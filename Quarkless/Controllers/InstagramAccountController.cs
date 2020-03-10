using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quarkless.Base.InstagramUser;
using Quarkless.Logic.InstagramClient;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
    public class InstagramAccountController : ControllerBaseExtended
    {
		private readonly IUserContext _userContext;
		private readonly IInstaUserLogic _instaUserLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IResponseResolver _responseResolver;
		private readonly IApiClientContext _clientContext;
		public InstagramAccountController(IUserContext userContext, 
			IInstagramAccountLogic instagramAccountLogic, 
			IInstaUserLogic instaUserLogic, 
			IResponseResolver responseResolver, IApiClientContext clientContext)
		{
			_userContext = userContext;
			_instagramAccountLogic = instagramAccountLogic;
			_instaUserLogic = instaUserLogic;
			_responseResolver = responseResolver;
			_clientContext = clientContext;
		}

		[HttpPut]
		[Route("api/insta/challange/submitCode/{code}")]
		public async Task<IActionResult> SubmitChallengeCode([FromRoute] string code)
		{
			if(!_userContext.UserAccountExists)
				return BadRequest("Invalid Request");

			var instagramAccountDetails =
				await _instagramAccountLogic.GetInstagramAccount(_userContext.CurrentUser,
					_userContext.FocusInstaAccount);
			if (instagramAccountDetails == null)
				return NotFound("Account was not found");
			
			var res = await _instaUserLogic.SubmitChallengeCode(instagramAccountDetails.Username,
				instagramAccountDetails.Password, instagramAccountDetails.ChallengeInfo?.ChallengePath, code);

			if (!res.Result.Succeeded) return NotFound(res.Result.Info);
			
			if (string.IsNullOrEmpty(res.InstagramId)) return Ok(true);
			
			await _instagramAccountLogic.EmptyChallengeInfo(res.InstagramId);
			return Ok(true);
		}

		[HttpPut]
		[Route("api/insta/challenge/submitPhone/{phoneNumber}")]
		public async Task<IActionResult> SubmitPhoneChallenge([FromRoute] string phoneNumber)
		{
			if (!_userContext.UserAccountExists)
				return BadRequest("Invalid Request");
			if (string.IsNullOrEmpty(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^[+]{0,1}\d+$"))
				return BadRequest("Invalid Number");

			var instagramAccountDetails =
				await _instagramAccountLogic.GetInstagramAccount(_userContext.CurrentUser,
					_userContext.FocusInstaAccount);

			if (instagramAccountDetails == null)
				return NotFound("Account was not found");


			var result =
				await _instaUserLogic.SubmitPhoneVerify(phoneNumber,
					instagramAccountDetails.ChallengeInfo?.ChallengePath);

			if (!result) return NotFound("failed to add phone");

			await _instagramAccountLogic.EmptyChallengeInfo(_userContext.FocusInstaAccount);
			return Ok(true);
		}

		[HttpDelete]
		[Route("api/insta/delete/{instagramAccountId}")]
		public async Task<IActionResult> RemoveInstagramAccount(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Not authenticated");
			try
			{
				var results = await _instagramAccountLogic.RemoveInstagramAccount(instagramAccountId);
				if(results)
					return Ok("Account Deleted");

				return NotFound("Account failed to delete");
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return BadRequest("Something went wrong");
			}
		}

		[HttpPost]
		[Route("api/insta/add")]
		public async Task<IActionResult> AddInstagramAccount(AddInstagramAccountRequest addRequest)
		{
			try
			{
				if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Not authenticated");
				
				var response = await _instagramAccountLogic.AddInstagramAccount(_userContext.CurrentUser, addRequest);
				if (!response.IsSuccessful)
					return BadRequest(response.Info);

				try
				{
					//if successful then try to login

					var clientContainer = new ApiClientContainer(_clientContext,
						response.Results.AccountId, response.Results.InstagramAccountId);
					
					if (clientContainer.GetContext.SuccessfullyRetrieved)
						return Ok(response);
					
					if (clientContainer.GetContext.Container == null)//exception was thrown or account failed add to the db
						goto Failed;

					if (clientContainer.GetContext.Response.Info.NeedsChallenge)
					{
						var responseR = await _responseResolver
							.WithClient(clientContainer)
							.WithResolverAsync(clientContainer.GetContext.Response);
						//challenge was handled
						if (responseR.Response.Succeeded)
						{
							return Ok(response);
						}
						
						// if requires user's input (e.g. phone verify, email, captcha)
						return OkChallengeResponse(responseR.HandlerResults.ChallengeResponse);
					}
				}
				catch(Exception err)
				{
					await _instagramAccountLogic.RemoveInstagramAccount(response.Results.InstagramAccountId);
					return BadRequest(err.Message);
				}

				Failed:
				//delete everything that was added if failed
				await _instagramAccountLogic.RemoveInstagramAccount(response.Results.InstagramAccountId);
				return BadRequest("Could not authenticate the account");
			}
			catch(Exception ee)
			{
				return BadRequest(ee.Message);
			}
		}

		[HttpGet]
		[Route("api/insta/refreshLogin/{instagramAccountId}")]
		public async Task<IActionResult> RefreshLogin(string instagramAccountId)
		{
			try
			{
				if (string.IsNullOrEmpty(_userContext.CurrentUser)) return Forbid("User does not exist");
				var instaDetails = await _instagramAccountLogic.GetInstagramAccount(_userContext.CurrentUser,instagramAccountId);
				
				if (instaDetails == null || string.IsNullOrEmpty(instaDetails.Username))
					return NotFound("Could not find account");

				var clientContainer =
					new ApiClientContainer(_clientContext, _userContext.CurrentUser, instagramAccountId);

				await _instagramAccountLogic.ClearCacheData(instaDetails.AccountId, instaDetails._id);

				var loginRes = await _responseResolver
					.WithClient(clientContainer)
					.WithAttempts(1)
					.WithResolverAsync(() => clientContainer.GetContext.Container.InstaClient
							.TryLogin(instaDetails.Username, instaDetails.Password, instaDetails.DeviceDetail,
								clientContainer.GetContext.Container.Proxy));
				
				if (loginRes == null) return Ok(false);

				if (!loginRes.Response.Succeeded)
				{
					if (loginRes.Response.Info.NeedsChallenge)
					{
						return OkChallengeResponse(loginRes.HandlerResults.ChallengeResponse);
					}

					return NotFound(loginRes.Response.Info);
				}

				var newState = JsonConvert.DeserializeObject<StateData>(loginRes.Response.Value);
				
				await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser,
					instagramAccountId,
					new InstagramAccountModel
					{
						State = newState,
						AgentState = (int) AgentState.Running
					});

				return Ok(true);
			}
			catch(Exception ee)
			{
				return BadRequest(ee.Message);
			}
		}

		[HttpGet]
		[Route("api/insta/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetInstagramAccount(string accountId, string instagramAccountId)
		{
			if (_userContext.CurrentUser == null || instagramAccountId == null)
				return BadRequest("Please populate the id");
			var results = await _instagramAccountLogic.GetInstagramAccount(accountId,instagramAccountId);
			return Ok(results);
		}

		[HttpGet]
		[Route("api/insta/state/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetInstagramAccountStateData(string accountId, string instagramAccountId)
		{
			if (_userContext.CurrentUser == null) return BadRequest("Please provide valid parameters");
			var results = await _instagramAccountLogic.GetInstagramAccountStateData(accountId,instagramAccountId);
			if(results!=null)
				return Ok(results);
			return NotFound($"Could not find state for instagram account: {instagramAccountId}");
		}

		[HttpGet]
		[Route("api/insta/{accountId}")]
		public async Task<IActionResult> GetInstagramAccounts(string accountId)
		{
			if(_userContext.CurrentUser != null && _userContext.UserRoleLevel.Equals(AuthTypes.Admin))
			{
				var results = await _instagramAccountLogic.GetInstagramAccountsOfUser(accountId);
				return Ok(results);
			}

			if (_userContext.CurrentUser == null || _userContext.CurrentUser != accountId)
				return BadRequest("Please populate the id");
			{
				var results = await _instagramAccountLogic.GetInstagramAccountsOfUser(_userContext.CurrentUser);
				return Ok(results);
			}
		}
		
		[HttpPut]
		[Route("api/insta/agent/{instagramAccountId}/{newstate}")]
		public async Task<IActionResult> UpdateAgentState(string instagramAccountId, int newState)
		{
			if (_userContext.CurrentUser == null) return BadRequest("failed to update");
			var res = await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser, instagramAccountId, new InstagramAccountModel
			{
				AgentState = newState
			});
			if (res.HasValue)
			{
				return Ok("updated");
			}
			return BadRequest("failed to update");
		}

		[HttpPut]
		[Route("api/insta/partial/{instagramAccountId}")]
		public async Task<IActionResult> PartialUpdateInstagramAccount(string instagramAccountId, [FromBody] InstagramAccountModel instagramAccount)
		{
			if (_userContext.CurrentUser == null) return BadRequest("Please provide valid id");
			var res = await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser,instagramAccountId, instagramAccount);
			if (res != null)
			{
				return Ok($"Modified: {res}");
			}
			return BadRequest("Please provide valid id");
		}
	}
}
using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.ResponseLogic;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]

	public class InstagramAccountController : ControllerBase
    {
		private readonly IUserContext _userContext;
		private readonly IInstaUserLogic _instaUserLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IResponseResolver _responseResolver;
		public InstagramAccountController(IUserContext userContext, IInstagramAccountLogic instagramAccountLogic, 
			IInstaUserLogic instaUserLogic, IResponseResolver responseResolver)
		{
			_userContext = userContext;
			_instagramAccountLogic = instagramAccountLogic;
			_instaUserLogic = instaUserLogic;
			_responseResolver = responseResolver;
		}
		[HttpPut]
		[Route("api/insta/challange/submitCode/{code}")]
		public async Task<IActionResult> SubmitChallangeCode([FromBody]SubmitVerifyCodeRequest model,[FromRoute] string code)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			var res = await _instaUserLogic.SubmitChallangeCode(model.Username, model.Password, model.ChallangeLoginInfo, code);
			if (!res.Result.Succeeded) return NotFound(res.Result.Info);
			if (string.IsNullOrEmpty(res.InstagramId)) return Ok(true);
			await _instagramAccountLogic.EmptyChallengeInfo(res.InstagramId);
			return Ok(true);
		}
		[HttpPost]
		[Route("api/insta/add")]
		public async Task<IActionResult> AddInstagramAccount(AddInstagramAccountRequest addInstagram)
		{
			try
			{
				if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Not authenticated");
				var loginRes = await _instaUserLogic.TryLogin(addInstagram.Username, addInstagram.Password);
				if (loginRes == null) return NotFound("Could not authenticate the account");
				if (loginRes.Succeeded) { 
					var state = JsonConvert.DeserializeObject<StateData>(await _instaUserLogic.GetStateDataFromString());
					var results = await _instagramAccountLogic.AddInstagramAccount(_userContext.CurrentUser,state,addInstagram);
					if (results.Results!=null)
					{
						return Ok(results.Results);
					}
				}
				else
				{
					if (loginRes.Value != InstaLoginResult.ChallengeRequired) return NotFound(loginRes.Info);
					var challange = await _instaUserLogic.GetChallengeRequireVerifyMethodAsync(addInstagram.Username, addInstagram.Password);
					if (!challange.Succeeded) return NotFound(loginRes.Info);
					if (challange.Value.SubmitPhoneRequired)
					{

					}
					else
					{
						if (challange.Value.StepData == null) return NotFound(loginRes.Info);
						if (!string.IsNullOrEmpty(challange.Value.StepData.PhoneNumber))
						{
							//verify phone
							var code = await _instaUserLogic.RequestVerifyCodeToSMSForChallengeRequireAsync(addInstagram.Username, addInstagram.Password);
							if (code.Succeeded)
							{
								return Ok(new 
								{ 
									Verify = "phone", 
									Details = challange.Value.StepData.PhoneNumber,
									ChallangePath = _instaUserLogic.GetChallangeInfo(),
									Resp = code.Value
								});
							}
						}

						if (string.IsNullOrEmpty(challange.Value.StepData.Email))
							return NotFound(loginRes.Info);
						{
							var code = await _instaUserLogic.RequestVerifyCodeToEmailForChallengeRequireAsync(addInstagram.Username, addInstagram.Password);
							return Ok(new { 
								Verify = "email", 
								Details = challange.Value.StepData.Email,
								ChallangePath = _instaUserLogic.GetChallangeInfo(),
								Resp = code.Value });
						}
					}
				}
				return NotFound(loginRes.Info);
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
				var loginRes = await _instaUserLogic.TryLogin(instaDetails.Username, instaDetails.Password);
				if (loginRes == null) return Ok(false);
				if (!loginRes.Succeeded) return NotFound(loginRes.Info);
				var newState = JsonConvert.DeserializeObject<StateData>(await _instaUserLogic.GetStateDataFromString());
				await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser,instagramAccountId,
					new InstagramAccountModel
					{
						State = newState
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
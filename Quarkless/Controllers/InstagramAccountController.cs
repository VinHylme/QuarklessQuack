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

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]

	public class InstagramAccountController : BaseController
    {
		private readonly IUserContext _userContext;
		private readonly IInstaUserLogic _instaUserLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		public InstagramAccountController(IUserContext userContext, IInstagramAccountLogic instagramAccountLogic, IInstaUserLogic instaUserLogic)
		{
			_userContext = userContext;
			_instagramAccountLogic = instagramAccountLogic;
			_instaUserLogic = instaUserLogic;
		}

		[HttpPost]
		[Route("api/insta/add")]
		public async Task<IActionResult> AddInstagramAccount(AddInstagramAccountRequest addInstagram)
		{
			try
			{
				if (!string.IsNullOrEmpty(_userContext.CurrentUser))
				{
					if(await _instaUserLogic.TryLogin(addInstagram.Username, addInstagram.Password)){
						var state = JsonConvert.DeserializeObject<StateData>(await _instaUserLogic.GetStateDataFromString());
						var results = await _instagramAccountLogic.AddInstagramAccount(_userContext.CurrentUser,state,addInstagram);
						if (results.Results)
						{
							return Ok(results.Results);
						}
						return BadRequest("Something went wrong on our side");
					}
					return NotFound("Could not authenticate the account");
				}
				return BadRequest("Not authenticated");
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
				if (!string.IsNullOrEmpty(_userContext.CurrentUser))
				{
					var instaDetails = await _instagramAccountLogic.GetInstagramAccount(_userContext.CurrentUser,instagramAccountId);
					if(instaDetails!=null && !string.IsNullOrEmpty(instaDetails.Username)) { 
						if(await _instaUserLogic.TryLogin(instaDetails.Username, instaDetails.Password))
						{
							var newState = JsonConvert.DeserializeObject<StateData>(await _instaUserLogic.GetStateDataFromString());
							await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser,instagramAccountId,
								new InstagramAccountModel
								{
									State = newState
								});
							return Ok(true);
						}
						return Ok(false);
					}
					return NotFound("Could not find account");
				}
				return Forbid("User does not exist");
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
			if (_userContext.CurrentUser != null && instagramAccountId!=null)
			{
				var results = await _instagramAccountLogic.GetInstagramAccount(accountId,instagramAccountId);
				return Ok(results);
			}
			return BadRequest("Please populate the id");
		}

		[HttpGet]
		[Route("api/insta/state/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetInstagramAccountStateData(string accountId, string instagramAccountId)
		{
			if(_userContext.CurrentUser != null)
			{
				var results = await _instagramAccountLogic.GetInstagramAccountStateData(accountId,instagramAccountId);
				if(results!=null)
					return Ok(results);
				else
					return NotFound($"Could not find state for instagram account: {instagramAccountId}");
			}
			return BadRequest("Please provide valid parameters");
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
			else if (_userContext.CurrentUser != null && _userContext.CurrentUser == accountId)
			{
				var results = await _instagramAccountLogic.GetInstagramAccountsOfUser(_userContext.CurrentUser);
				return Ok(results);
			}
			return BadRequest("Please populate the id");
		}
		
		[HttpPut]
		[Route("api/insta/agent/{instagramAccountId}/{newstate}")]
		public async Task<IActionResult> UpdateAgentState(string instagramAccountId, int newState)
		{
			if (_userContext.CurrentUser!=null)
			{
				var res = await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser, instagramAccountId, new InstagramAccountModel
				{
					AgentState = newState
				});
				if (res.HasValue)
				{
					return Ok("updated");
				}
			}
			return BadRequest("failed to update");
		}

		[HttpPut]
		[Route("api/insta/partial/{instagramAccountId}")]
		public async Task<IActionResult> PartialUpdateInstagramAccount(string instagramAccountId, [FromBody] InstagramAccountModel instagramAccount)
		{
			if (_userContext.CurrentUser != null)
			{
				var res = await _instagramAccountLogic.PartialUpdateInstagramAccount(_userContext.CurrentUser,instagramAccountId, instagramAccount);
				if (res != null)
				{
					return Ok($"Modified: {res}");
				}
			}
			return BadRequest("Please provide valid id");
		}
	}
}
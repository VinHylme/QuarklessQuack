using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.ProfileLogic;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.Admin)]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	public class ProfilesController : ControllerBase
    {
		private readonly IUserContext _userContext;
		private readonly IProfileLogic _profileLogic;
		public ProfilesController(IUserContext userContext, IProfileLogic profileLogic)
		{
			_userContext = userContext;
			_profileLogic = profileLogic;
		}

		[HttpPost]
		[Route("api/profiles")]
		public async Task<IActionResult> AddProfile(ProfileModel profile)
		{
			if (_userContext.CurrentUser == null) return BadRequest("User not valid");
			if (profile?.Account_Id != null && profile.InstagramAccountId !=null)
			{
				return Ok(await _profileLogic.AddProfile(profile));
			}
			return BadRequest("Please provide the correct format");
		}

		[HttpGet]
		[Route("api/profiles/{accountId}")]
		public async Task<IActionResult> GetProfiles(string accountId)
		{
			if (_userContext.CurrentUser == null) return BadRequest("Please state yourself");
			if (!_userContext.CurrentUser.Equals(accountId)) return BadRequest("Please state yourself");
			var profiles = await _profileLogic.GetProfiles(accountId);
			if(profiles!=null)
				return Ok(profiles);
			return NotFound($"Could not find any profiles belonging to {accountId}");
		}

		[HttpGet]
		[Route("api/profiles/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetProfile(string accountId, string instagramAccountId)
		{
			if (_userContext.CurrentUser == null) return BadRequest("Please state yourself");
			var profile = await _profileLogic.GetProfile(accountId,instagramAccountId);
			if (profile != null)
			{
				return Ok(profile);
			}
			return NotFound($"Could not find any profile for {instagramAccountId}");
		}

		[HttpPut]
		[Route("api/profiles/partial/{profileId}")]
		public async Task<IActionResult> UpdateInstagramAccountPartial(string profileId, [FromBody] ProfileModel profileModel)
		{
			if (_userContext.CurrentUser == null) return BadRequest("Please provide valid id");
			var res = await _profileLogic.PartialUpdateProfile(profileId, profileModel);
			if (res != null)
			{
				return Ok($"Modified: {res}");
			}
			return BadRequest("Please provide valid id");
		}
	}
}
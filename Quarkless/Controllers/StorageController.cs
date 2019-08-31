using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.StorageLogic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.Admin)]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	public class StorageController : ControllerBase
	{
		private readonly IS3BucketLogic _s3BucketLogic;
		private readonly IUserContext _userContext;
		public StorageController(IUserContext userContext, IS3BucketLogic s3BucketLogic)
		{
			_s3BucketLogic = s3BucketLogic;
			_userContext = userContext;
		}

		[HttpPut]
		[Route("api/storage/upload/{instagramAccountId}/{profileId}")]
		public async Task<IActionResult> UploadImages(string instagramAccountId, string profileId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			if (string.IsNullOrEmpty(instagramAccountId) || string.IsNullOrEmpty(profileId)) return BadRequest("Invalid account");
			if (Request.Form.Files == null || Request.Form.Files.Count <= 0) return BadRequest("No files");
			IList<string> results = new List<string>();
			foreach (var file in Request.Form.Files)
			{
				if (file.Length <= 0) continue;
				var keyName = $"{_userContext.CurrentUser}:{instagramAccountId}:{profileId}:{file.Name}";
				var res = await _s3BucketLogic.UploadStreamFile(file.OpenReadStream(), keyName: keyName);
				if (string.IsNullOrEmpty(res)) return BadRequest("Something went wrong");
				results.Add(res);
			}
			return Ok(new { Urls = results });
		}
	}
}

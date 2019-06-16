using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.MediaModels;
using QuarklessLogic.Logic.MediaLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class MediaController : ControllerBase
	{
		private readonly IMediaLogic _mediaLogic;
		private readonly IUserContext _userContext;
		public MediaController(IUserContext userContext, IMediaLogic mediaLogic)
		{
			_mediaLogic = mediaLogic;
			_userContext = userContext;
		}

		[HttpPost]
		[Route("api/media/upload/photo")]
		public async Task<IActionResult> UploadPhoto([FromBody]UploadPhotoModel uploadPhoto)
		{
			if (_userContext.UserAccountExists && uploadPhoto != null)
			{
				var results = await _mediaLogic.UploadPhotoAsync(uploadPhoto);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
	}
}

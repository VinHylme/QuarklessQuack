using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.InstaUserLogic;
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
		private readonly IInstaUserLogic _instaUserLogic;
		private readonly IUserContext _userContext;
		public MediaController(IUserContext userContext, IMediaLogic mediaLogic, IInstaUserLogic instaUserLogic)
		{
			_instaUserLogic = instaUserLogic;
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
				else
				{
					if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
					{
						await _instaUserLogic.AcceptConsent();
					}
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPost]
		[Route("api/media/upload/carousel")]
		public async Task<IActionResult> UploadCarousel([FromBody]UploadAlbumModel uploadAlbum)
		{
			if (_userContext.UserAccountExists && uploadAlbum != null)
			{
				var results = await _mediaLogic.UploadAlbumAsync(uploadAlbum);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				else
				{
					if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
					{
						await _instaUserLogic.AcceptConsent();
					}
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPost]
		[Route("api/media/upload/video")]
		public async Task<IActionResult> UploadVideo([FromBody] UploadVideoModel uploadVideo)
		{
			if (_userContext.UserAccountExists && uploadVideo != null)
			{
				var results = await _mediaLogic.UploadVideoAsync(uploadVideo);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				else
				{
					if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
					{
						await _instaUserLogic.AcceptConsent();
					}
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}

		[HttpPost]
		[Route("api/media/delete/{mediaId}/{mediaType}")]
		public async Task<IActionResult> DeleteMedia(string mediaId, int mediaType)
		{
			if (_userContext.UserAccountExists && mediaId != null)
			{
				var results = await _mediaLogic.DeleteMediaAsync(mediaId,mediaType);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				else
				{
					if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
					{
						await _instaUserLogic.AcceptConsent();
					}
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}

		[HttpPost]
		[Route("api/media/like/{mediaId}")]
		public async Task<IActionResult> LikeMedia(string mediaId)
		{
			if (_userContext.UserAccountExists && !string.IsNullOrEmpty(mediaId))
			{
			var results = await _mediaLogic.LikeMediaAsync(mediaId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				else
				{
					if(results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
					{
						await _instaUserLogic.AcceptConsent();
					}
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}

	}
}

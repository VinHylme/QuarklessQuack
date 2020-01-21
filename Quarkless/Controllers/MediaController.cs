using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Media;
using Quarkless.Models.Media.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;

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
		private readonly IResponseResolver _responseResolver;
		public MediaController(IUserContext userContext, IMediaLogic mediaLogic, IResponseResolver responseResolver)
		{
			_mediaLogic = mediaLogic;
			_userContext = userContext;
			_responseResolver = responseResolver;
		}

		[HttpPost]
		[Route("api/media/upload/photo")]
		public async Task<IActionResult> UploadPhoto([FromBody]UploadPhotoModel uploadPhoto)
		{
			if (!_userContext.UserAccountExists || uploadPhoto == null) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _mediaLogic.UploadPhotoAsync(uploadPhoto), ActionType.CreatePost, uploadPhoto.ToJsonString());
			if (!results.Succeeded) return NotFound(results.Info);
			return Ok(results.Value);
		}
		[HttpPost]
		[Route("api/media/upload/carousel")]
		
		public async Task<IActionResult> UploadCarousel([FromBody]UploadAlbumModel uploadAlbum)
		{
			if (!_userContext.UserAccountExists || uploadAlbum == null) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _mediaLogic.UploadAlbumAsync(uploadAlbum), ActionType.CreatePost, uploadAlbum.ToJsonString());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPost]
		[Route("api/media/upload/video")]
		public async Task<IActionResult> UploadVideo([FromBody] UploadVideoModel uploadVideo)
		{
			if (!_userContext.UserAccountExists || uploadVideo == null) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _mediaLogic.UploadVideoAsync(uploadVideo), ActionType.CreatePost, uploadVideo.ToJsonString());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}

		[HttpPost]
		[Route("api/media/delete/{mediaId}/{mediaType}")]
		public async Task<IActionResult> DeleteMedia(string mediaId, int mediaType)
		{
			if (!_userContext.UserAccountExists || mediaId == null) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _mediaLogic.DeleteMediaAsync(mediaId,mediaType), ActionType.None, mediaId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return NotFound(results.Info);
		}

		[HttpPost]
		[Route("api/media/like/{mediaId}")]
		public async Task<IActionResult> LikeMedia(string mediaId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId)) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _mediaLogic.LikeMediaAsync(mediaId), ActionType.LikePost, mediaId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}

	}
}

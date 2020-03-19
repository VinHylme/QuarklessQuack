using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.Media.Models;
using Quarkless.Base.Media.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class MediaController : ControllerBaseExtended
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
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _mediaLogic.UploadPhotoAsync(uploadPhoto), ActionType.CreatePost, uploadPhoto);
			return ResolverResponse(results);
		}
		[HttpPost]
		[Route("api/media/upload/carousel")]
		
		public async Task<IActionResult> UploadCarousel([FromBody]UploadAlbumModel uploadAlbum)
		{
			if (!_userContext.UserAccountExists || uploadAlbum == null) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _mediaLogic.UploadAlbumAsync(uploadAlbum), ActionType.CreatePost, uploadAlbum);
			return ResolverResponse(results);
		}
		[HttpPost]
		[Route("api/media/upload/video")]
		public async Task<IActionResult> UploadVideo([FromBody] UploadVideoModel uploadVideo)
		{
			if (!_userContext.UserAccountExists || uploadVideo == null) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _mediaLogic.UploadVideoAsync(uploadVideo), ActionType.CreatePost, uploadVideo);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/media/delete/{mediaId}/{mediaType}")]
		public async Task<IActionResult> DeleteMedia(string mediaId, int mediaType)
		{
			if (!_userContext.UserAccountExists || mediaId == null) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _mediaLogic.DeleteMediaAsync(mediaId,mediaType));
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/media/like/{mediaId}")]
		public async Task<IActionResult> LikeMedia(string mediaId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId)) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _mediaLogic.LikeMediaAsync(mediaId));
			return ResolverResponse(results);
		}
	}
}

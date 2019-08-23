using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.MediaLogic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessLogic.Logic.TimelineEventLogLogic;

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
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		public MediaController(IUserContext userContext, IMediaLogic mediaLogic, 
			IInstaUserLogic instaUserLogic, ITimelineEventLogLogic timelineEventLogLogic)
		{
			_instaUserLogic = instaUserLogic;
			_mediaLogic = mediaLogic;
			_userContext = userContext;
			_timelineEventLogLogic = timelineEventLogLogic;
		}

		[HttpPost]
		[Route("api/media/upload/photo")]
		public async Task<IActionResult> UploadPhoto([FromBody]UploadPhotoModel uploadPhoto)
		{
			if (!_userContext.UserAccountExists || uploadPhoto == null) return BadRequest("invalid");
			var results = await _mediaLogic.UploadPhotoAsync(uploadPhoto);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Uploaded Photo {results.Value.Url} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request =  JsonConvert.SerializeObject(uploadPhoto),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response =  JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}

			if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Upload Photo Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}] Consent Required",
					Request =  JsonConvert.SerializeObject(uploadPhoto),
					Response =  JsonConvert.SerializeObject(results.Info),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.FeedbackRequired,
					Level = 2
				});
				await _instaUserLogic.AcceptConsent();
				return BadRequest("Consent Required");
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.CreatePost,
				Message = $"Upload Photo Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request =  JsonConvert.SerializeObject(uploadPhoto),
				Response =  JsonConvert.SerializeObject(results.Info),
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.Failed,
				Level = 2
			});
			return NotFound(results.Info);
		}
		[HttpPost]
		[Route("api/media/upload/carousel")]
		public async Task<IActionResult> UploadCarousel([FromBody]UploadAlbumModel uploadAlbum)
		{
			if (!_userContext.UserAccountExists || uploadAlbum == null) return BadRequest("invalid");
			var results = await _mediaLogic.UploadAlbumAsync(uploadAlbum);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Uploaded Carousel {results.Value.Url} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request =  JsonConvert.SerializeObject(uploadAlbum),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response =  JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}

			if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Upload Carousel Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}] Consent Required",
					Request =  JsonConvert.SerializeObject(uploadAlbum),
					Response =  JsonConvert.SerializeObject(results.Info),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.FeedbackRequired,
					Level = 2
				});
				await _instaUserLogic.AcceptConsent();
				return BadRequest("Consent Required");
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.CreatePost,
				Message = $"Upload Carousel Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request =  JsonConvert.SerializeObject(uploadAlbum),
				Response =  JsonConvert.SerializeObject(results.Info),
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.FeedbackRequired,
				Level = 2
			});
			return NotFound(results.Info);
		}
		[HttpPost]
		[Route("api/media/upload/video")]
		public async Task<IActionResult> UploadVideo([FromBody] UploadVideoModel uploadVideo)
		{
			if (!_userContext.UserAccountExists || uploadVideo == null) return BadRequest("invalid");
			var results = await _mediaLogic.UploadVideoAsync(uploadVideo);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Uploaded Video {results.Value.Url} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request =  JsonConvert.SerializeObject(uploadVideo),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response =  JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}

			if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Upload Video Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}] Consent Required",
					Request =  JsonConvert.SerializeObject(uploadVideo),
					Response =  JsonConvert.SerializeObject(results.Info),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.FeedbackRequired,
					Level = 2
				});
				await _instaUserLogic.AcceptConsent();
				return BadRequest("Consent Required");
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.CreatePost,
				Message = $"Upload Video Failed {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request =  JsonConvert.SerializeObject(uploadVideo),
				Response =  JsonConvert.SerializeObject(results.Info),
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.FeedbackRequired,
				Level = 2
			});
			return NotFound(results.Info);
		}

		[HttpPost]
		[Route("api/media/delete/{mediaId}/{mediaType}")]
		public async Task<IActionResult> DeleteMedia(string mediaId, int mediaType)
		{
			if (!_userContext.UserAccountExists || mediaId == null) return BadRequest("invalid");
			var results = await _mediaLogic.DeleteMediaAsync(mediaId,mediaType);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			if (results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
			{
				await _instaUserLogic.AcceptConsent();
			}
			return NotFound(results.Info);
		}

		[HttpPost]
		[Route("api/media/like/{mediaId}")]
		public async Task<IActionResult> LikeMedia(string mediaId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId)) return BadRequest("invalid");
			var results = await _mediaLogic.LikeMediaAsync(mediaId);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.LikePost,
					Message = $"Liked Media {mediaId} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request = mediaId,
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response =  JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}

			if(results.Info.ResponseType == InstagramApiSharp.Classes.ResponseType.ConsentRequired)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreatePost,
					Message = $"Failed to like media: {mediaId} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}] Consent Required",
					Request = mediaId,
					Response =  JsonConvert.SerializeObject(results.Info),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.FeedbackRequired,
					Level = 2
				});
				await _instaUserLogic.AcceptConsent();
				return BadRequest("Consent Required");
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.CreatePost,
				Message = $"Failed to like media: {mediaId} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request = mediaId,
				Response =  JsonConvert.SerializeObject(results.Info),
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.Failed,
				Level = 2
			});
			return NotFound(results.Info);
		}

	}
}

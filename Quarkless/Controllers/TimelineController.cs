using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Messaging;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class TimelineController : ControllerBase
	{
		private readonly IUserContext _userContext;
		private readonly ITimelineLogic _timelineLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		public TimelineController(IUserContext userContext, ITimelineLogic timelineLogic,
			ITimelineEventLogLogic timelineEventLogLogic)
		{
			_userContext = userContext;
			_timelineLogic = timelineLogic;
			_timelineEventLogLogic = timelineEventLogLogic;
		}

		[HttpGet]
		[Route("api/timeline/log/{instagramAccountId}/{limit}")]
		public async Task<IActionResult> GetTimelineEventLog(string instagramAccountId, int limit)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			return Ok(await _timelineEventLogLogic.GetLogsForUser(_userContext.CurrentUser, instagramAccountId, limit));
		}

		[HttpGet]
		[Route("api/timeline/log/{limit}")]
		public async Task<IActionResult> GetTimelineEventLog(int limit)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			if (string.IsNullOrEmpty(_userContext.FocusInstaAccount)) return BadRequest("Invalid user");
			return Ok(await _timelineEventLogLogic.GetLogsForUser(_userContext.CurrentUser, _userContext.FocusInstaAccount, limit));
		}

//		[HashtagAuthorize(AuthTypes.Admin)]
//		[HttpGet]
//		[Route("api/timeline/log/all/{actionType}")]
//		public async Task<IActionResult> GetAllTimelineEventLog(int actionType = 0)
//		{
//			if (!_userContext.IsAdmin)
//				return BadRequest("Invalid Request");
//			return Ok(await _timelineEventLogLogic.GetAllTimelineLogs((ActionType)actionType));
//		}

		[HashtagAuthorize(AuthTypes.Admin)]
		[HttpGet]
		[Route("api/timeline/{from}/{limit}")]
		public IActionResult GetEvents(int from = 0, int limit = 1)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest();
			var res = _timelineLogic.GetTotalScheduledEvents(@from,limit);
			return Ok(res);
		}

		[HttpGet]
		[Route("api/timeline/{eventId}")]
		public IActionResult GetEvent(string eventId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest();
			var res = _timelineLogic.GetEventDetail(eventId);
			return Ok(res);
		}

		[HttpGet]
		[Route("api/schedule/timeline/{instagramId}")]
		public IActionResult GetEventForUser(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				//List<ResultBase<TimelineItemShort>> res = _timelineLogic.ShortGetAllEventsForUser(_userContext.CurrentUser,DateTime.UtcNow,instaId:instagramId,timelineDateType:TimelineDateType.Forward).ToList();			
				return Ok(_timelineLogic.GetScheduledPosts(_userContext.CurrentUser,instagramId));
			}
			return BadRequest();
		}

		[HttpDelete]
		[Route("api/timeline/delete/{eventId}")]
		public IActionResult DeleteEvent(string eventId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid user");
			var res = _timelineLogic.DeleteEvent(eventId);
			if(res)
				return Ok("Deleted");

			return NotFound("this event no longer exists or has already been deleted");
		}

		[HttpPut]
		[Route("api/timeline/update")]
		public IActionResult UpdateEvent(UpdateTimelineMediaItemRequest updateTimelineMediaItemRequest)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Cannot access this");
			try
			{
				var newId = _timelineLogic.UpdateEvent(updateTimelineMediaItemRequest);
				if(!string.IsNullOrEmpty(newId))
					return Ok(newId);

				return BadRequest("Could not Update event");
			}
			catch(Exception ee)
			{
				return BadRequest(ee.Message);
			}
		}

		[HttpPut]
		[Route("api/timeline/enqueue/{eventId}")]
		public IActionResult EnqueueNow(string eventId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid user");
			try { 
				_timelineLogic.ExecuteNow(eventId);
				return Ok("Added");
			}
			catch(Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpPost]
		[Route("api/timeline/post/{instagramId}")]
		public async Task<IActionResult> CreatePost([FromRoute] string instagramId, [FromBody] RawMediaSubmit dataMediaSubmit)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			if (dataMediaSubmit == null) return BadRequest("Data Invalid");
			if (dataMediaSubmit.Hashtags == null && dataMediaSubmit.Hashtags?.Count <= 0)
				return BadRequest("Invalid Hashtags");
			if (dataMediaSubmit.ExecuteTime <= DateTime.UtcNow) return BadRequest("Invalid Time");
			if (string.IsNullOrEmpty(dataMediaSubmit.Caption)) return BadRequest("Invalid Caption");
			if (dataMediaSubmit.RawMediaDatas == null || dataMediaSubmit.RawMediaDatas?.Count()<=0)
				return BadRequest("Invalid Media");

			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var results = await _timelineLogic.SchedulePostsByUser(user, dataMediaSubmit);
			if (results.IsSuccessful)
			{
				return Ok(results);
			}
			
			return BadRequest(results.NumberOfFails);
		}

		[HttpPost]
		[Route("api/timeline/messaging/photo/{instagramId}")]
		public async Task<IActionResult> CreatePhotoMessages([FromRoute] string instagramId, [FromBody] IEnumerable<SendDirectPhotoModel> messages)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			if (messages == null) return BadRequest("Invalid Params");
			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var schedule = await _timelineLogic.ScheduleMessage(user, messages);
			if(!schedule.IsSuccessful)
				return NotFound(schedule);

			return Ok(schedule);
		}

		[HttpPost]
		[Route("api/timeline/messaging/video/{instagramId}")]
		public async Task<IActionResult> CreateVideoMessages([FromRoute] string instagramId, [FromBody] IEnumerable<SendDirectVideoModel> messages)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			if (messages == null) return BadRequest("Invalid Params");
			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var schedule = await _timelineLogic.ScheduleMessage(user, messages);
			if(!schedule.IsSuccessful)
				return NotFound(schedule);

			return Ok(schedule);
		}

		[HttpPost]
		[Route("api/timeline/messaging/text/{instagramId}")]
		public async Task<IActionResult> CreateTextMessages([FromRoute] string instagramId, [FromBody] IEnumerable<SendDirectTextModel> messages)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			if (messages == null) return BadRequest("Invalid Params");
			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var schedule = await _timelineLogic.ScheduleMessage(user, messages);
			if(!schedule.IsSuccessful)
				return NotFound(schedule);

			return Ok(schedule);
		}

		[HttpPost]
		[Route("api/timeline/messaging/link/{instagramId}")]
		public async Task<IActionResult> CreateLinkMessages([FromRoute] string instagramId, [FromBody] IEnumerable<SendDirectLinkModel> messages)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			if (messages == null) return BadRequest("Invalid Params");
			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var schedule = await _timelineLogic.ScheduleMessage(user, messages);
			if(!schedule.IsSuccessful)
				return NotFound(schedule);

			return Ok(schedule);
		}

		[HttpPost]
		[Route("api/timeline/messaging/media/{instagramId}")]
		public async Task<IActionResult> CreateShareMediaMessage([FromRoute] string instagramId, [FromBody] IEnumerable<ShareDirectMediaModel> messages)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			if (messages == null) return BadRequest("Invalid Params");
			var user = new UserStoreDetails
			{
				AccountId = _userContext.CurrentUser,
				InstagramAccountUser = instagramId
			};

			var schedule = await _timelineLogic.ScheduleMessage(user, messages);
			if(!schedule.IsSuccessful)
				return NotFound(schedule);

			return Ok(schedule);
		}
		
		[HttpPost]
		[Route("api/timeline/{instagramId}")]
		public IActionResult AddEvent([FromRoute] string instagramId, [FromBody] EventActionOptions eventItem)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("something went wrong");
			_userContext.FocusInstaAccount = instagramId;
			try {
				_timelineLogic.AddEventToTimeline(eventItem);
				return Ok("Added to queue");
			}
			catch(Exception ee) {
				return BadRequest(ee.Message);
			}
		}
	}
}

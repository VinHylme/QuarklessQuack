using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.Timeline;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using System;
using System.Collections.Generic;
using System.Linq;

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
		private readonly IRequestBuilder _requestBuilder;
		private readonly ITimelineLogic _timelineLogic;
		public TimelineController(IUserContext userContext, ITimelineLogic timelineLogic, IRequestBuilder requestBuilder)
		{
			_userContext = userContext;
			_timelineLogic = timelineLogic;
			_requestBuilder = requestBuilder;
		}
		[HashtagAuthorize(AuthTypes.Admin)]
		[HttpGet]
		[Route("api/timeline/{from}/{limit}")]
		public IActionResult GetEvents(int from = 0, int limit = 1)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var res = _timelineLogic.GetTotalScheduledEvents(from,limit);
				return Ok(res);
			}
			return BadRequest();
		}

		[HttpGet]
		[Route("api/timeline/{eventId}")]
		public IActionResult GetEvent(string eventId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var res = _timelineLogic.GetEventDetail(eventId);
				return Ok(res);
			}
			return BadRequest();
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
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var res = _timelineLogic.DeleteEvent(eventId);
				if(res)
					return Ok("Deleted");

				return NotFound("this event no longer exists or has already been deleted");
			}
			return BadRequest("Invalid user");
		}
		[HttpPut]
		[Route("api/timeline/update")]
		public IActionResult UpdateEvent(UpdateTimelineItemRequest updateTimelineItemRequest)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				try
				{
					var newId = _timelineLogic.UpdateEvent(updateTimelineItemRequest);
					if(!string.IsNullOrEmpty(newId))
						return Ok(newId);

					return BadRequest("Could not Update event");
				}
				catch(Exception ee)
				{
					return BadRequest(ee.Message);
				}
			}
			return BadRequest("Cannot access this");
		}
		[HttpPut]
		[Route("api/timeline/enqueue/{eventId}")]
		public IActionResult EnqueueNow(string eventId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				try { 
					_timelineLogic.ExecuteNow(eventId);
					return Ok("Added");
				}
				catch(Exception e)
				{
					return BadRequest(e.Message);
				}
			}
			return BadRequest("Invalid user");
		}
		[HttpPost]
		[Route("api/timeline/{instagramId}")]
		public IActionResult AddEvent([FromRoute] string instagramId, [FromBody] LongRunningJobOptions eventItem)
		{

			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var accessToken = HttpContext.Request.Headers["Authorization"];
				_userContext.FocusInstaAccount = instagramId;
				var headers = _requestBuilder.DefaultHeaders(instagramId); //token will expire after 1 hour, either increase token time or find another solution
				try {
					eventItem.Rest.RequestHeaders.ToList().AddRange(headers);
					eventItem.Rest.User.OAccessToken = accessToken;
					_timelineLogic.AddEventToTimeline(eventItem.ActionName,eventItem.Rest, eventItem.ExecutionTime);
					return Ok("Added to queue");
				}
				catch(Exception ee) {
					return BadRequest(ee.Message);
				}
			}

			return BadRequest("something went wrong");
		}
	}
}

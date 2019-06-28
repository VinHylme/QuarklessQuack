using Quarkless.Queue.Jobs.Interfaces;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public class TimelineLogic : ITimelineLogic
	{
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		public TimelineLogic(ITaskService taskService, IRequestBuilder requestBuilder)
		{
			_taskService = taskService;
			_requestBuilder = requestBuilder;
		}

		public bool AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders.AddRange(
				_requestBuilder.DefaultHeaders(
				restBody.User.OInstagramAccountUser,
				restBody.User.OAccessToken));

			var eventId = _taskService.ScheduleEvent(actionName, restBody, executeTime);
			return eventId!=null;
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username,string instaId = null, int limit = 100)
		{
			return GetScheduledEventsForUser(username,instaId,limit:limit).Where(_=>_.ActionName.Split('_')[0].ToLower().Equals(actionName.ToLower()));
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username , DateTime date, DateTime? endDate = null, string instaId = null, int limit=100)
		{
			if (endDate == null)
			{ 
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}

			var events = GetScheduledEventsForUser(username,instaId,limit:limit).Where(_=>
			_.EnqueueTime >= date && _.EnqueueTime <= endDate);

			return events;
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit=100)
		{
			if (endDate == null)
			{
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}

			var events = GetFinishedEventsForUser(username, instaId, limit: limit).Where(_ =>
			  _.SuccededAt >= date && _.SuccededAt <= endDate);

			return events;
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, int limit=100, string instaid=null)
		{
			if (endDate == null)
			{
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}

			var events = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit).Where(_ =>
			  _.StartedAt >= date && _.StartedAt <= endDate);

			return events;
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100)
		{
			if (endDate == null)
			{
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}

			var events = GetDeletedEventsForUser(username, instaId, limit: limit).Where(_ =>
			  _.DeletedAt >= date && _.DeletedAt <= endDate);

			return events;
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100)
		{
			if (endDate == null)
			{
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}

			var events = GetFailedEventsForUser(username, instaId, limit: limit).Where(_ =>
			  _.FailedAt >= date && _.FailedAt <= endDate);

			return events;
		}
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit=1000)
		{
			if(endDate==null)
				endDate = startDate.AddDays(1).AddTicks(-1);
			List<ResultBase<TimelineItem>> totalEvents = new List<ResultBase<TimelineItem>>();

			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName,startDate,endDate,instaId,limit).Select(_=>new ResultBase<TimelineItem>{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineItem)
			}));
			totalEvents.AddRange(GetFinishedEventsForUserByDate(userName,startDate,endDate,instaId,limit).Select(_=>new  ResultBase<TimelineItem>
			{
				Response = new TimelineItem { 
					ActionName = _.ActionName,
					EnqueueTime = _.SuccededAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Results,
				TimelineType = typeof(TimelineFinishedItem)
			}));
			totalEvents.AddRange(GetCurrentlyRunningEventsForUserByDate(userName,startDate,endDate,limit,instaId).Select(_=>new ResultBase<TimelineItem>
			{ 
				Response = new TimelineItem { 
					ActionName = _.ActionName,
					EnqueueTime = _.StartedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineInProgressItem)
			}));
			totalEvents.AddRange(GetDeletedEventsForUserByDate(userName, startDate, endDate, instaId,limit).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.DeletedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineDeletedItem)
			}));
			totalEvents.AddRange(GetFailedEventsForUserByDate(userName, startDate, endDate, instaId, limit).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.FailedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Error,
				TimelineType = typeof(TimelineFailedItem)
			}));

			return totalEvents;
		}
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000)
		{
			return GetAllEventsForUser(userName,startDate,endDate,instaId,limit).Where(_=>_?.Response?.ActionName?.Split('_')?[0]?.ToLower()==(actionName.ToLower()));
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username,string actionName, DateTime date, DateTime? endDate = null)
		{
			if (endDate == null) { 
				endDate = date.AddDays(1).AddTicks(-1); // until the end of the date (23:59:59)
			}
			var events = GetScheduledEventsForUser(username, limit: 100).Where(_ =>
			  (_.EnqueueTime >= date && _.EnqueueTime <= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower()==(actionName.ToLower()));

			return events;
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetCurrentlyRunningItemsForUser(username,instagramId,limit);
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFailedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetDeletedItemsForUser(username, instagramId, limit);
		}
		public TimelineItemDetail GetEventDetail(string eventId)
		{
			return _taskService.GetEvent(eventId);
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFinishedItemsForUser(username,instagramId,limit);
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetScheduledItemsForUser(username,instagramId,limit);
		}
		public TimelineStatistics GetTimelineStatistics()
		{
			return _taskService.GetStatistics();
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit)
		{
			return _taskService.GetTotalCurrentlyRunningJobs(from,limit);
		}
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedEvents(int from, int limit)
		{
			return _taskService.GetTotalFinishedJobs(from, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			return _taskService.GetTotalDeletedEvents(from, limit);
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			return _taskService.GetTotalFailedEvents(from, limit);
		}
		public IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit)
		{
			return _taskService.GetTotalScheduledEvents(from,limit);
		}
		public bool IsAnyEventsCurrentlyRunning()
		{
			return _taskService.IsAnyJobsCurrentlyRunning();
		}
	}
}

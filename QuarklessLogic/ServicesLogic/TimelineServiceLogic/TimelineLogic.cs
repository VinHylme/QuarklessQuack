using Quarkless.Queue.Jobs.Interfaces;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessRepositories.Repository.TimelineRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public enum TimelineDateType
	{
		Backwards,
		Forward,
		Both
	}
	public class TimelineLogic : ITimelineLogic
	{
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		private readonly ITimelineRepository _timelineRepository;
		public TimelineLogic(ITaskService taskService, IRequestBuilder requestBuilder, ITimelineRepository timelineRepository)
		{
			_taskService = taskService;
			_requestBuilder = requestBuilder;
			_timelineRepository = timelineRepository;
		}
		#region Add Event To Queue
		public bool AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders.AddRange(
				_requestBuilder.DefaultHeaders(
				restBody.User.OInstagramAccountUser,
				restBody.User.OAccessToken));

			var eventId = _taskService.ScheduleEvent(actionName, restBody, executeTime);
			return eventId != null;
		}
		#endregion

		#region Get Specified UserAction
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username, string instaId = null, int limit = 100)
		{
			return GetScheduledEventsForUser(username, instaId, limit: limit).Where(_ => _.ActionName.Split('_')[0].ToLower().Equals(actionName.ToLower()));
		}
		#endregion
		#region GET BY USING DATES
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName,
			DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			return GetAllEventsForUser(userName, startDate, endDate, instaId, limit, timelineDateType).Where(_ => _?.Response?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime <= date && _.EnqueueTime >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime >= date && _.EnqueueTime <= endDate);
					return eventsF;
				case TimelineDateType.Both:
					return GetScheduledEventsForUser(username, instaId, limit: limit);
			}
			return null;
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFinishedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.SuccededAt <= date && _.SuccededAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFinishedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.SuccededAt >= date && _.SuccededAt <= endDate);
					return eventsF;
				case TimelineDateType.Both:
					return GetFinishedEventsForUser(username, instaId, limit: limit);
			}
			return null;
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			int limit = 100, string instaid = null, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt <= date && _.StartedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt >= date && _.StartedAt <= endDate);
					return eventsF;
				case TimelineDateType.Both:
					return GetCurrentlyRunningEventsForUser(username, instaid, limit: limit);
			}
			return null;
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt <= date && _.DeletedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt >= date && _.DeletedAt <= endDate);
					return eventsF;
				case TimelineDateType.Both:
					return GetDeletedEventsForUser(username, instaId, limit: limit);
			}
			return null;
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt <= date && _.FailedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt >= date && _.FailedAt <= endDate);
					return eventsF;
				case TimelineDateType.Both:
					return GetFailedEventsForUser(username, instaId, limit: limit);
			}
			return null;
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, string actionName, DateTime date,
			string instaId = null, DateTime? endDate = null, int limit = 30, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime <= date && _.EnqueueTime >= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime >= date && _.EnqueueTime <= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsF;
				case TimelineDateType.Both:
					return GetScheduledEventsForUser(username, instaId, limit: limit)
					.Where(_ => _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
			}
			return null;
		}
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
			string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			List<ResultBase<TimelineItem>> totalEvents = new List<ResultBase<TimelineItem>>();
			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
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
			totalEvents.AddRange(GetFinishedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
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
			totalEvents.AddRange(GetCurrentlyRunningEventsForUserByDate(userName, startDate, endDate, limit, instaId, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.StartedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineInProgressItem)
			}));
			totalEvents.AddRange(GetDeletedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
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
			totalEvents.AddRange(GetFailedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
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

		#endregion
		#region GETTING EVENT DETAILS FOR THE USER
		public IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetScheduledItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFinishedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetCurrentlyRunningItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFailedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetDeletedItemsForUser(username, instagramId, limit);
		}
		#endregion
		#region GET BY SPECIFIED JOB ID FOR ANY USER (ADMIN) BUT CAN BE USED FOR USER LEVEL TOO (BECAREFUL)
		public TimelineItemDetail GetEventDetail(string eventId)
		{
			return _taskService.GetEvent(eventId);
		}
		#endregion
		#region ADMIN TIMELINE OPTIONS
		public TimelineStatistics GetTimelineStatistics()
		{
			return _taskService.GetStatistics();
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit)
		{
			return _taskService.GetTotalCurrentlyRunningJobs(from, limit);
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
			return _taskService.GetTotalScheduledEvents(from, limit);
		}
		public bool IsAnyEventsCurrentlyRunning()
		{
			return _taskService.IsAnyJobsCurrentlyRunning();
		}
		#endregion
	}
}

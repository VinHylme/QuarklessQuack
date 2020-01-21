using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Messaging.Interfaces;
using Quarkless.Models.Timeline.Enums;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineLogic
	{
		Task<TimelineScheduleResponse<RawMediaSubmit>> SchedulePostsByUser(UserStoreDetails userStoreDetails,
			RawMediaSubmit dataMediaSubmit);
		Task<TimelineScheduleResponse<IEnumerable<IDirectMessageModel>>> ScheduleMessage(UserStoreDetails userStoreDetails, IEnumerable<IDirectMessageModel> messages);
		DateTime PickAGoodTime(string accountId, string instagramAccountId, ActionType? actionName = null);
		string UpdateEvent(UpdateTimelineItemRequest updateTimelineItemRequest);
		bool DeleteEvent(string eventId);
		void ExecuteNow(string eventId);
		string AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime);
		IEnumerable<TimelineItemShort> GetScheduledPosts(string username, string instagramId, int limit = 1000);
		IEnumerable<ResultBase<TimelineItemShort>> ShortGetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
		string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, int limit = 100, string instaid = null, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		TimelineItemDetail GetEventDetail(string eventId);
		IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username, string instaId = null, int limit = 100);
		IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, string actionName, DateTime date, string instaId = null, DateTime? endDate = null, int limit = 30, TimelineDateType timelineDateType = TimelineDateType.Backwards);
		TimelineStatistics GetTimelineStatistics();
		IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit);
		IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit);
		IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit);
		IEnumerable<TimelineFinishedItem> GetTotalFinishedEvents(int from, int limit);
		IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit);
		bool IsAnyEventsCurrentlyRunning();
	}
}
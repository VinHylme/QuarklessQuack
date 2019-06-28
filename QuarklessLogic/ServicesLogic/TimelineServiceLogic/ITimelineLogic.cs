using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public interface ITimelineLogic
	{
		bool AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime);
		IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000);
		IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username, string instaId = null, int limit = 100);
		IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100);
		IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100);
		IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, int limit = 100, string instaid = null);
		IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100);
		IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null, string instaId = null, int limit = 100);
		IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000);
		IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, string actionName, DateTime date, DateTime? endDate = null);
		IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30);
		TimelineItemDetail GetEventDetail(string eventId);
		IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30);
		TimelineStatistics GetTimelineStatistics();

		//ADMINS ONLY
		IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit);
		IEnumerable<TimelineFinishedItem> GetTotalFinishedEvents(int from, int limit);
		IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit);
		IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit);
		IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit);
		bool IsAnyEventsCurrentlyRunning();
	}
}
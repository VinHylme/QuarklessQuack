using System;
using System.Collections.Generic;
using Quarkless.Base.Timeline.Models.TaskScheduler;

namespace Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler
{
	public interface ITaskService
	{
		void ResetJobs();
		string ScheduleEvent(EventActionOptions eventAction);
		//string ScheduleEvent(string actionName, RestModel restModel, DateTimeOffset timeOffset);
		void ActionTask(Delegate @delegate, DateTimeOffset executeTime, params object[] args);
		IEnumerable<TimelineItem> GetScheduledItemsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineInProgressItem> GetCurrentlyRunningItemsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFinishedItem> GetFinishedItemsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineDeletedItem> GetDeletedItemsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFailedItem> GetFailedItemsForUser(string username, string instagramId = null, int limit = 30);
		IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit);
		IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit);
		IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit);
		IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningJobs(int from, int limit);
		IEnumerable<TimelineFinishedItem> GetTotalFinishedJobs(int from, int limit);
		void ExecuteNow(string jobid);
		TimelineStatistics GetStatistics();
		bool IsAnyJobsCurrentlyRunning();
		bool DeleteEvent(string eventId);
		TimelineItemDetail GetEvent(string eventId);
	}
}

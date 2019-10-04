using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.TimelineLoggingRepository;

namespace QuarklessRepositories.Repository.TimelineRepository
{
	public interface ITimelineLoggingRepository
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId);
//		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId);
//		Task<IEnumerable<TimelineEventLog>> GetAllTimelineLogs(ActionType actionType = ActionType.None);
	}
}
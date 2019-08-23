using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.TimelineLoggingRepository;

namespace QuarklessLogic.Logic.TimelineEventLogLogic
{
	public interface ITimelineEventLogLogic
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, int limit);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId, int limit);
		Task<IEnumerable<TimelineEventLog>> GetAllTimelineLogs(ActionType actionType = ActionType.None);
	}
}
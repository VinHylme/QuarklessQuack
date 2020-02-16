using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Timeline.Enums;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineLoggingRepositoryMongo
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId,
			int limit = 250, int severityLevel = 1, TimelineEventStatus? status = null);

		Task<IEnumerable<TimelineEventLog>> GetLogsForUserFromActionType(
			string accountId, string instagramAccountId, ActionType actionType);
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineLoggingRepository
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId);
	}
}

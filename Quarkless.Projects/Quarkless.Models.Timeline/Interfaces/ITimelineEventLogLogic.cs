using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineEventLogLogic
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId, int limit);

		Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types);
		Task<IEnumerable<TimelineEventLog>> GetLogsByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types);
	}
}

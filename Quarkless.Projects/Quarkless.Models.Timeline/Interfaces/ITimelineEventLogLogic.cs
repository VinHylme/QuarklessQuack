using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.Timeline.Enums;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineEventLogLogic
	{
		Task AddTimelineLogFor(TimelineEventLog timelineEvent);
		Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId,
			int limit, int level = 1, TimelineEventStatus? status = null);
		Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types);
		Task<IEnumerable<TimelineEventLog>> GetLogsByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types);
	}
}

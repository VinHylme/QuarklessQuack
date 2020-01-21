using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Logic.Timeline
{
	public class TimelineEventLogLogic : ITimelineEventLogLogic
	{
		private readonly ITimelineLoggingRepository _timelineLoggingRepository;
		public TimelineEventLogLogic(ITimelineLoggingRepository timelineLoggingRepository) =>
			_timelineLoggingRepository = timelineLoggingRepository;

		public async Task AddTimelineLogFor(TimelineEventLog timelineEvent)
		{
			await _timelineLoggingRepository.AddTimelineLogFor(timelineEvent);
		}

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId, int limit)
		{
			var res = (await _timelineLoggingRepository.GetLogsForUser(accountId, instagramAccountId))
				.Where(p => p.Level == 1)
				.OrderByDescending(_ => _.DateAdded)
				.Take(limit);
			return res;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessRepositories.Repository.TimelineRepository;

namespace QuarklessLogic.Logic.TimelineEventLogLogic
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

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, int limit)
		{
			var res = (await _timelineLoggingRepository.GetLogsForUser(accountId))
				.Where(p=>p.Level == 1)
				.OrderByDescending(_=>_.DateAdded)
				.Take(limit);
			return res;
		}

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId, int limit)
		{
			return (await _timelineLoggingRepository.GetLogsForUser(accountId, instagramAccountId)).Take(limit);
		}

		public async Task<IEnumerable<TimelineEventLog>> GetAllTimelineLogs(ActionType actionType = ActionType.None)
		{
			return await _timelineLoggingRepository.GetAllTimelineLogs(actionType);
		}
	}
}

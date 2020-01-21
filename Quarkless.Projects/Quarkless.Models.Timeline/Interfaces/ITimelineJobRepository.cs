using System.Collections.Generic;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface ITimelineJobRepository
	{
		List<EventResponse> GetScheduledJobs(int from, int limit);
	}
}
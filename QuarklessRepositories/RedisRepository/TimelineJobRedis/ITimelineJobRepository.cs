using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire.Storage.Monitoring;
using static QuarklessRepositories.RedisRepository.TimelineJobRedis.TimelineJobRepository;

namespace QuarklessRepositories.RedisRepository.TimelineJobRedis
{
	public interface ITimelineJobRepository
	{
		List<EventResponse> GetScheduledJobs(int from, int limit);
	}
}
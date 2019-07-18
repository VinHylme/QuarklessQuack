using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.TimelineJobRedis
{
	class TimelineJobRepository
	{
		private readonly IRedisClient _redis;
		public TimelineJobRepository(IRedisClient redis)
		{
			_redis = redis;
		}
 

	}
}

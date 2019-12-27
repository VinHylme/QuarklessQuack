using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuarklessContexts.Models.APILogger;
using QuarklessRepositories.RedisRepository.RedisClient;

namespace QuarklessRepositories.RedisRepository.APILogger
{
	public class APILogCache : IAPILogCache
	{
		private readonly IRedisClient _redis;
		public APILogCache(IRedisClient redis) => _redis = redis;

		public async Task LogData(ApiLogMetaData apiLogMeta)
		{
			try
			{
				await _redis.SetAdd("Users",RedisKeys.HashtagGrowKeys.ApiLog, JsonConvert.SerializeObject(apiLogMeta),
					TimeSpan.FromDays(80));
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		public async Task<IEnumerable<ApiLogMetaData>> GetAllLogData()
		{
			return await _redis.GetMembers<ApiLogMetaData>("Users", RedisKeys.HashtagGrowKeys.ApiLog);
			//return JsonConvert.DeserializeObject<IEnumerable<ApiLogMetaData>>(await _redis.StringGet(RedisKeys.HashtagGrowKeys.ApiLog));
		}
	}
}

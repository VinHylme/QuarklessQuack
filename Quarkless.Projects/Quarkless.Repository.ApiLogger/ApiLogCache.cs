using Newtonsoft.Json;
using Quarkless.Models.ApiLogger;
using Quarkless.Models.ApiLogger.Interfaces;
using Quarkless.Repository.RedisContext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Repository.ApiLogger
{
	public class ApiLogCache : IApiLogCache
	{
		private readonly IRedisClient _redis;
		public ApiLogCache(IRedisClient redis) => _redis = redis;

		public async Task LogData(ApiLogMetaData apiLogMeta)
		{
			try
			{
				await _redis.SetAdd("Users", RedisKeys.HashtagGrowKeys.ApiLog, JsonConvert.SerializeObject(apiLogMeta),
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

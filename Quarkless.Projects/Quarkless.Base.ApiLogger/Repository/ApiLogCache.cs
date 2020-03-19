using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Base.ApiLogger.Models;
using Quarkless.Base.ApiLogger.Models.Interfaces;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Base.ApiLogger.Repository
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

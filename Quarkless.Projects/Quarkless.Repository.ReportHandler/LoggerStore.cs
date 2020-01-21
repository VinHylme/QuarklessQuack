using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Models.ReportHandler;
using Quarkless.Models.ReportHandler.Enums;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Repository.ReportHandler
{
	public class LoggerStore : ILoggerStore
	{
		private readonly IRedisClient _redis;
		public LoggerStore(IRedisClient redis) => _redis = redis;
		public async Task Log(LoggerModel loggerModel)
		{
			await WithExceptionLogAsync(async () =>
			{
				await _redis.SetAdd("HashtagGrow:" + RedisKeys.HashtagGrowKeys.Logger,
					JsonConvert.SerializeObject(loggerModel), TimeSpan.FromDays(2));
			});
		}
		public async Task<IEnumerable<LoggerModel>> GetLog(string accountId, string instagramAccountId = null)
		{
			IEnumerable<LoggerModel> results = null;
			if (accountId == null) return null;
			await WithExceptionLogAsync(async () =>
			{
				var res = (await _redis.GetMembers<LoggerModel>("HashtagGrow:" + RedisKeys.HashtagGrowKeys.Logger.ToString()));
				if (string.IsNullOrEmpty(instagramAccountId))
					results = res.Where(x => x.AccountId == accountId);
				else
					results = res.Where(x => x.AccountId == accountId && x.InstagramAccountId == instagramAccountId);

			});
			return results;
		}
		public async Task<IEnumerable<LoggerModel>> GetLogBySection(string section, SeverityLevel severityLevel = SeverityLevel.All)
		{
			IEnumerable<LoggerModel> results = null;
			if (string.IsNullOrEmpty(section)) return null;
			await WithExceptionLogAsync(async () =>
			{
				var res = (await _redis.GetMembers<LoggerModel>("HashtagGrow:" + RedisKeys.HashtagGrowKeys.Logger.ToString()));
				results = severityLevel == SeverityLevel.All ? res.Where(x => x.Section == section) : res.Where(x => x.SeverityLevel == severityLevel && x.Section == section);
			});
			return results;
		}
		public async Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All)
		{
			IEnumerable<LoggerModel> results = null;
			await WithExceptionLogAsync(async () =>
			{
				var res = (await _redis.GetMembers<LoggerModel>("HashtagGrow:" + RedisKeys.HashtagGrowKeys.Logger.ToString()));
				if (severityLevel == SeverityLevel.All)
					results = res;
				else
					results = res.Where(x => x.SeverityLevel == severityLevel);
			});
			return results;
		}
		private Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
	}

}

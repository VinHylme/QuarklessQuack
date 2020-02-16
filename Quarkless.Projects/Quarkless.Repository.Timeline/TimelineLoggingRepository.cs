using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Timeline;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Repository.Timeline
{
	public class TimelineLoggingRepository : ITimelineLoggingRepository
	{
		private readonly IRedisClient _redisClient;
		public TimelineLoggingRepository(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}

		public async Task AddTimelineLogFor(TimelineEventLog timelineEvent)
		{
			try
			{
				var userId = $"{timelineEvent.AccountId}:{timelineEvent.InstagramAccountId}";
				await _redisClient.SetAdd(userId, RedisKeys.HashtagGrowKeys.TimelineLog,
					JsonConvert.SerializeObject(timelineEvent), TimeSpan.FromHours(24));
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}
		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId)
		{
			try
			{
				var userId = $"{accountId}:{instagramAccountId}";
				return await _redisClient.GetMembers<TimelineEventLog>(userId,
					RedisKeys.HashtagGrowKeys.TimelineLog);
			}
			catch
			{
				return new List<TimelineEventLog>();
			}
		}
	}
}

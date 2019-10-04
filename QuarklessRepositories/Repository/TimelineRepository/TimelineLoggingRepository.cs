using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessRepositories.RedisRepository.RedisClient;
using QuarklessRepositories.RepositoryClientManager;

namespace QuarklessRepositories.Repository.TimelineRepository
{
	public class TimelineLoggingRepository : ITimelineLoggingRepository
	{
		private readonly IRepositoryContext _context;
		private readonly IRedisClient _redisClient;
		public TimelineLoggingRepository(IRepositoryContext context, IRedisClient redisClient)
		{
			_context = context;
			_redisClient = redisClient;
		}

		public async Task AddTimelineLogFor(TimelineEventLog timelineEvent)
		{
			try
			{
				var userId = $"{timelineEvent.AccountID}:{timelineEvent.InstagramAccountID}";
				await _redisClient.SetAdd(userId, RedisKeys.HashtagGrowKeys.TimelineLog,
					JsonConvert.SerializeObject(timelineEvent), TimeSpan.FromHours(24));
				//await _context.TimelineLogger.InsertOneAsync(timelineEvent);
			}
			catch(Exception ee)
			{
				// ignored
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
			catch (Exception ex)
			{
				return new List<TimelineEventLog>();
			}
		}

//		[Obsolete]
//		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId)
//		{
//			var builder = Builders<TimelineEventLog>.Filter;
//			var filters = builder.Eq(_ => _.AccountID, accountId)  & builder.Eq(_=>_.InstagramAccountID,instagramAccountId);
//			return (await _context.TimelineLogger.FindAsync(filters)).ToList();
//		}
//		[Obsolete]
//		public async Task<IEnumerable<TimelineEventLog>> GetAllTimelineLogs(ActionType actionType = ActionType.None)
//		{
//			var builders = Builders<TimelineEventLog>.Filter;
//			var filters = builders.Empty; 
//			if (actionType == ActionType.None)
//				filters = builders.Eq(_ => _.ActionType, actionType);
//			return (await _context.TimelineLogger.FindAsync(filters)).ToList();
//		}
	}
}

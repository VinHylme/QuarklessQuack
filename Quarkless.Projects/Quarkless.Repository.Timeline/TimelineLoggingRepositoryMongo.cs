using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using MongoDB.Driver;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Timeline
{
	public class TimelineLoggingRepositoryMongo : ITimelineLoggingRepositoryMongo
	{
		private readonly IMongoCollection<TimelineEventLog> _ctx;
		private const string COLLECTION_NAME = "TimelineActionLogs";

		public TimelineLoggingRepositoryMongo(IMongoClientContext context)
			=> _ctx = context.StatisticsDatabase.GetCollection<TimelineEventLog>(COLLECTION_NAME);

		public async Task AddTimelineLogFor(TimelineEventLog timelineEvent)
		{
			try
			{
				timelineEvent.ExpiresAt = DateTime.UtcNow.AddHours(24);
				await _ctx.InsertOneAsync(timelineEvent);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId,
			int limit = 250, int severityLevel = 1, TimelineEventStatus? status = null)
		{
			try
			{
				var findOptions = new FindOptions<TimelineEventLog, TimelineEventLog>
				{
					Limit = limit,
					Sort = Builders<TimelineEventLog>.Sort.Descending(_ => _.DateAdded)
				};

				var filterWithStatus = new ExpressionFilterDefinition<TimelineEventLog>
					(_ => _.AccountId == accountId && _.InstagramAccountId == instagramAccountId
						&& _.Level == severityLevel && _.Status == status);

				var filterWithoutStatus = new ExpressionFilterDefinition<TimelineEventLog>
				(_ => _.AccountId == accountId && _.InstagramAccountId == instagramAccountId
					&& _.Level == severityLevel);

				var filter = status == null ? filterWithoutStatus : filterWithStatus;

				var results = await _ctx.FindAsync(filter, findOptions);

				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<TimelineEventLog>();
			}
		}

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUserFromActionType(
			string accountId, string instagramAccountId, ActionType actionType)
		{
			try
			{
				var results = await _ctx.FindAsync(_ =>
					_.AccountId == accountId && _.InstagramAccountId == instagramAccountId && _.ActionType == actionType);

				return results.ToList();
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<TimelineEventLog>();
			}
		}
	}
}
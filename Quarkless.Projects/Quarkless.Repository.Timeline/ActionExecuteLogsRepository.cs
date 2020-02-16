using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Timeline
{
	public class ActionExecuteLogsRepository : IActionExecuteLogsRepository
	{
		private readonly IMongoCollection<ActionExecuteLog> _ctx;
		private const string COLLECTION_NAME = "ActionExecuteLog";

		public ActionExecuteLogsRepository(IMongoClientContext context)
			=> _ctx = context.StatisticsDatabase.GetCollection<ActionExecuteLog>(COLLECTION_NAME);

		public async Task AddActionLog(ActionExecuteLog log)
		{
			try
			{
				await _ctx.InsertOneAsync(log);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}
		public async Task<IEnumerable<ActionExecuteLog>> GetActionLogs(string accountId, string instagramAccountId, int limit = 250)
		{
			try
			{
				var findOptions = new FindOptions<ActionExecuteLog, ActionExecuteLog>
				{
					Limit = limit,
					Sort = Builders<ActionExecuteLog>.Sort.Descending(_ => _.DateAdded)
				};

				var results = await _ctx.FindAsync(_=>_.AccountId == accountId && _.InstagramAccountId == instagramAccountId, findOptions);

				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<ActionExecuteLog>();
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Repository.RedisContext;

namespace Quarkless.Repository.Timeline
{
	public class TimelineJobRepository : ITimelineJobRepository
	{
		private readonly IRedisClient _redis;
		public TimelineJobRepository(IRedisClient redis)
		{
			_redis = redis;
		}
		public List<EventResponse> GetScheduledJobs(int from, int limit)
		{
			var response = new List<EventResponse>();
			var jobs = _redis.Database(1).GetKeys(limit).ToList();
			foreach(var job in jobs)
			{
				try
				{
					if (job.ToString().Count(_ => _ == ':') != 1) continue;
					{
						var res =  _redis.Database(1).GetHash(job);
						var obj = res.SingleOrDefault(_ => _.Name.ToString().Contains("Arguments")).Value;
						var fetchExecuteTime = JObject.Parse(JsonConvert.DeserializeObject<IReadOnlyList<object>>(obj).FirstOrDefault()?.ToString())["ExecutionTime"].ToString();
						response.Add(new EventResponse
						{
							ItemId = job.ToString().Split(':')[0],
							EnqueueAt = DateTime.Parse(fetchExecuteTime),
							CreatedAt = DateTime.Parse(res.SingleOrDefault(_ => _.Name.ToString().Contains("CreatedAt")).Value),
							Culture = res.FirstOrDefault(_ => _.Name.ToString().Contains("Culture")).Value,
							JsonBody = obj,
							State = (res.SingleOrDefault(_ => _.Name.ToString().Contains("State")).Value)
						});
					}
				}
				catch
				{
					continue;
				}
			}
			return response.Where(_=>_.State == "Scheduled").Take(limit).ToList();
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

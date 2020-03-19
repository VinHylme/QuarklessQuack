using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Base.InstagramAccounts.Repository.Redis
{
	public class InstagramAccountRedis : IInstagramAccountRedis
	{
		private readonly IRedisClient _redisClient;
		public InstagramAccountRedis(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}

		public async Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountActiveDetail()
		{
			var results = new List<ShortInstagramAccountModel>();
			var res = _redisClient.Database(0).GetKeys(1000).Where(s => s.ToString().Split(':').ElementAtOrDefault(1) == RedisKeys.HashtagGrowKeys.UserSessions.ToString()).ToList();
			foreach (var _ in res)
			{
				var segragate = Regex.Match(_.ToString(), @"\(.*?\)").Value.Replace(")", "").Replace("(", "").Split(':');
				if (segragate == null || segragate.Length <= 1) continue;
				var userkeyid = $"{segragate[0]}:{segragate[1]}";
				var response = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions);
				if (string.IsNullOrEmpty(response)) continue;
				var objectIns = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(response);
				if (objectIns.AgentState == (int)AgentState.Running)
				{
					results.Add(objectIns);
				}
			};
			return results;
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetWorkerAccounts()
		{
			var results = new List<ShortInstagramAccountModel>();
			var res = _redisClient.Database(0).GetKeys(1000).Where(s => s.ToString().Split(':').ElementAtOrDefault(1) == RedisKeys.HashtagGrowKeys.UserSessions.ToString()).ToList();
			foreach (var _ in res)
			{
				var segragate = Regex.Match(_.ToString(), @"\(.*?\)").Value.Replace(")", "").Replace("(", "").Split(':');
				if (segragate == null || segragate.Length <= 1) continue;
				var userkeyid = $"{segragate[0]}:{segragate[1]}";
				var response = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions);
				if (string.IsNullOrEmpty(response)) continue;
				var objectIns = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(response);
				if (objectIns.Type == 1)
				{
					results.Add(objectIns);
				}
			};
			return results;
		}
		public async Task<ShortInstagramAccountModel> GetInstagramAccountDetail(string userId, string instaId)
		{
			ShortInstagramAccountModel user = null;
			var userkeyid = $"{userId}:{instaId}";
			var res = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions);
			if (!string.IsNullOrEmpty(res))
			{
				user = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(res);
			}
			return user;
		}
		public async Task<bool> AccountExists(string userId, string instaId)
		{
			var userkeyid = $"{userId}:{instaId}";
			return await _redisClient.Database(0).KeyExists(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions);
		}
		public async Task SetInstagramAccountDetail(string userId, string instaId, ShortInstagramAccountModel value)
		{
			var userkeyid = $"{userId}:{instaId}";
			var getcurrentinplace = await GetInstagramAccountDetail(userId, instaId) ?? new ShortInstagramAccountModel();
			var newvalue = value.CreateNewObjectIgnoringNulls(getcurrentinplace);
			await _redisClient.Database(0).StringSet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions, JsonConvert.SerializeObject(newvalue));
		}
	}
}

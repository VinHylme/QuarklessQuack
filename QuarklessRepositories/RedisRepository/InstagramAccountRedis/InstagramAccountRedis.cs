﻿using Newtonsoft.Json;
using Quarkless.Extensions;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessRepositories.RedisRepository.RedisClient;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.InstagramAccountRedis
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
			List<ShortInstagramAccountModel> results = new List<ShortInstagramAccountModel>();
			var res = _redisClient.Database(0).GetKeys(1000).Where(s=>s.ToString().Split(':').ElementAtOrDefault(1) == RedisKeys.HashtagGrowKeys.Usersessions.ToString()).ToList();
			foreach(var _ in res)
			{
				var segragate = Regex.Match(_.ToString(),@"\(.*?\)").Value.Replace(")","").Replace("(","").Split(':');
				if(segragate!=null && segragate.Length > 1) { 
					string userkeyid = $"{segragate[0]}:{segragate[1]}";
					var response = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
					if (!string.IsNullOrEmpty(response))
					{
						var objectIns = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(response);
						if(objectIns.AgentState == (int)AgentState.Running)
						{
							results.Add(objectIns);
						}
					}
				}
			};
			return results;
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetWorkerAccounts()
		{
			List<ShortInstagramAccountModel> results = new List<ShortInstagramAccountModel>();
			var res = _redisClient.Database(0).GetKeys(1000).Where(s => s.ToString().Split(':').ElementAtOrDefault(1) == RedisKeys.HashtagGrowKeys.Usersessions.ToString()).ToList();
			foreach (var _ in res)
			{
				var segragate = Regex.Match(_.ToString(), @"\(.*?\)").Value.Replace(")", "").Replace("(", "").Split(':');
				if (segragate != null && segragate.Length > 1)
				{
					string userkeyid = $"{segragate[0]}:{segragate[1]}";
					var response = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
					if (!string.IsNullOrEmpty(response))
					{
						var objectIns = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(response);
						if (objectIns.Type == 1)
						{
							results.Add(objectIns);
						}
					}
				}
			};
			return results;
		}
		public async Task<ShortInstagramAccountModel> GetInstagramAccountDetail(string userId, string instaId)
		{
			ShortInstagramAccountModel user = null;
			string userkeyid = $"{userId}:{instaId}";
			var res = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
			if (!string.IsNullOrEmpty(res))
			{
				user = JsonConvert.DeserializeObject<ShortInstagramAccountModel>(res);
			}
			return user;
		}
		public async Task<bool> AccountExists(string userId, string instaId)
		{
			string userkeyid = $"{userId}:{instaId}";
			return await _redisClient.Database(0).KeyExists(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
		}
		public async Task SetInstagramAccountDetail(string userId, string instaId, ShortInstagramAccountModel value)
		{
			string userkeyid = $"{userId}:{instaId}";
			var getcurrentinplace = await GetInstagramAccountDetail(userId,instaId);
			if(getcurrentinplace==null) getcurrentinplace = new ShortInstagramAccountModel();
			var newvalue = value.CreateNewObjectIgnoringNulls(getcurrentinplace);
			await _redisClient.Database(0).StringSet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions, JsonConvert.SerializeObject(newvalue));
		}
	}
}

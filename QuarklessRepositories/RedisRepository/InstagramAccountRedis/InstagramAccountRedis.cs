using Newtonsoft.Json;
using Quarkless.Extensions;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public async Task<ShortInstagramAccountModel> GetInstagramAccountDetail(string userId, string instaId)
		{
			string userkeyid = $"{userId}:{instaId}";
			var res = await _redisClient.StringGet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
			if (res != null && !string.IsNullOrEmpty(res))
			{
				return JsonConvert.DeserializeObject<ShortInstagramAccountModel>(res);
			}
			return null;
		}
		public async Task SetInstagramAccountDetail(string userId, string instaId, ShortInstagramAccountModel value)
		{
			string userkeyid = $"{userId}:{instaId}";
			var getcurrentinplace = await GetInstagramAccountDetail(userId,instaId);
			if(getcurrentinplace==null) getcurrentinplace = new ShortInstagramAccountModel();
			var newvalue = value.CreateNewObjectIgnoringNulls(getcurrentinplace);
			await _redisClient.StringSet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions, JsonConvert.SerializeObject(newvalue));
		}
	}
}

using Newtonsoft.Json;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Repository.RedisContext.Models;
using System.Threading.Tasks;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Repository.RedisContext;

namespace Quarkless.Repository.Auth
{
	public class AccountCache : IAccountCache
	{
		private readonly IRedisClient _redisClient;
		public AccountCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}
		public async Task<AccountUser> GetAccount(string userId)
		{
			var userkeyid = $"{userId}";
			var res = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions);
			if (res != null && !string.IsNullOrEmpty(res))
			{
				return JsonConvert.DeserializeObject<AccountUser>(res);
			}
			return null;
		}
		public async Task SetAccount(AccountUser value)
		{
			var userkeyid = $"{value.UserName}";
			await _redisClient.Database(0).StringSet(userkeyid, RedisKeys.HashtagGrowKeys.UserSessions, JsonConvert.SerializeObject(value), expiry: System.TimeSpan.FromHours(1));
		}
	}
}

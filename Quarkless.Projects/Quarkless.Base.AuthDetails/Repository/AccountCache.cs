using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Base.Auth.Common.Models.AccountContext;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Base.AuthDetails.Repository
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.Lookup.Models;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Base.Lookup.Repository
{
	public class LookupCache : ILookupCache
	{
		private readonly IRedisClient _redis;
		public LookupCache(IRedisClient redis) => _redis = redis;

		public async Task AddObjectToLookup(string accountId, string instagramAccountId, LookupModel lookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				var key = $"{accountId}:{instagramAccountId}";
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, lookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}

		public async Task UpdateObjectToLookup(string accountId, string instagramAccountId,
			LookupModel oldLookup, LookupModel newLookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				var key = $"{accountId}:{instagramAccountId}";
				await _redis.SetRemove(key, RedisKeys.HashtagGrowKeys.LookUp, oldLookup.ToJsonString());
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, newLookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupModel>> Get(string accountId, string instagramAccountId)
		{
			IEnumerable<LookupModel> response = null;
			await WithExceptionLogAsync(async () =>
			{
				var key = $"{accountId}:{instagramAccountId}";
				response = await _redis.GetMembers<LookupModel>(key, RedisKeys.HashtagGrowKeys.LookUp);

			}, accountId, instagramAccountId);

			if(response == null) return new List<LookupModel>();
			return response.OrderByDescending(x => x.LastModified);
		}

		private Task WithExceptionLogAsync(Func<Task> actionAsync, string accountId, string instagramId)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}: for user {accountId} of account {instagramId}");
				throw;
			}
		}
	}

}

using Quarkless.Models.Lookup;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Repository.RedisContext;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Objects;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Repository.Lookup
{
	public class LookupCache : ILookupCache
	{
		private readonly IRedisClient _redis;
		public LookupCache(IRedisClient redis) => _redis = redis;

		public async Task AddObjectToLookup(string accountId, string instagramAccountId, LookupModel lookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}";
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, lookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}

		public async Task UpdateObjectToLookup(string accountId, string instagramAccountId,
			LookupModel oldLookup, LookupModel newLookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}";
				await _redis.SetRemove(key, RedisKeys.HashtagGrowKeys.LookUp, oldLookup.ToJsonString());
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, newLookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupModel>> Get(string accountId, string instagramAccountId)
		{
			IEnumerable<LookupModel> response = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}";
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

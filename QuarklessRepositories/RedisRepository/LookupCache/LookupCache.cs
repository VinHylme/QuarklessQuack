using MoreLinq;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.LookupModels;
using QuarklessRepositories.RedisRepository.RedisClient;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.LookupCache
{
	public class LookupCache : ILookupCache
	{
		private readonly IRedisClient _redis;
		public LookupCache(IRedisClient redis) => _redis = redis;

		public async Task AddObjectToLookup(string accountId, string instagramAccountId, string objId, LookupModel lookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}:{objId}";
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, lookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}
		public async Task UpdateObjectToLookup(string accountId, string instagramAccountId, string objId, 
			LookupModel oldlookup, LookupModel newLookup)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}:{objId}";
				await _redis.SetRemove(key, RedisKeys.HashtagGrowKeys.LookUp, oldlookup.ToJsonString());
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.LookUp, newLookup.ToJsonString(), TimeSpan.FromDays(1.5));
			}, accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupModel>> Get(string accountId, string instagramAccountId, string objId)
		{
			IEnumerable<LookupModel> response = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramAccountId}:{objId}";
				response = await _redis.GetMembers<LookupModel>(key, RedisKeys.HashtagGrowKeys.LookUp);

			}, accountId, instagramAccountId);

			return response.OrderByDescending(x=>x.LastModified);
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

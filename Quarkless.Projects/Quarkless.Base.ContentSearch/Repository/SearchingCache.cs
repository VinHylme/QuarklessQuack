using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Models.Models;
using Quarkless.Models.Common.Extensions;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using StackExchange.Redis;

namespace Quarkless.Base.ContentSearch.Repository
{
	public class SearchingCache : ISearchingCache
	{
		private readonly IRedisClient _redis;
		public SearchingCache(IRedisClient redisClient)
		{
			_redis = redisClient;
		}

		public async Task<List<TResult>> GetSearchData<TResult>(string id)
		{
			var results = new List<TResult>();
			try
			{
				RedisKey key = $"{id}:{typeof(TResult).Name}";
				var res = await _redis.GetMembers<IEnumerable<TResult>>(key, RedisKeys.HashtagGrowKeys.SearchSession);
				results.AddRange(res.SelectMany(_ => _));
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}

			return results;
		}
		public async Task AddSearchData<TResult>(string id, IEnumerable<TResult> data)
		{
			try
			{
				RedisKey key = $"{id}:{typeof(TResult).Name}";
				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.SearchSession, data.ToJsonString(),
					TimeSpan.FromHours(1));
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}
		public async Task StoreSearchData(string userId, SearchRequest search, string instagramId = null, string profileId = null)
		{
			try
			{
				RedisKey key = $"{userId}";
				if (!string.IsNullOrEmpty(instagramId))
					key = key.Append($":{instagramId}");

				if (!string.IsNullOrEmpty(profileId))
					key = key.Append($":{profileId}");

				key = key.Append($":{search.Offset.ToString()}");

				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.SearchSession, JsonConvert.SerializeObject(search), TimeSpan.FromHours(1));
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		public async Task<SearchRequest> GetSearchData(string userId, SearchRequest find, string instagramId = null, string profileId = null)
		{
			try
			{
				RedisKey key = $"{userId}";
				if (!string.IsNullOrEmpty(instagramId))
					key = key.Append($":{instagramId}");

				if (!string.IsNullOrEmpty(profileId))
					key = key.Append($":{profileId}");

				key = key.Append($":{find.Offset.ToString()}");

				var res = await _redis.GetMembers<SearchRequest>(key, RedisKeys.HashtagGrowKeys.SearchSession);
				if (res == null) return null;
				var req = res.Select(s => s.RequestData).ToList();
				var findMe = req.FindIndex(s => s.SequenceEqual(find.RequestData));
				return findMe >= 0 ? res.ElementAtOrDefault(findMe) : null;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<SubTopics> GetRelatedTopic(string topic)
		{
			RedisKey key = $"RelatedTopics:{topic}";
			try
			{
				var res = await _redis.Database(2).GetMembers<SubTopics>(key, RedisKeys.HashtagGrowKeys.SearchSession);
				return res.FirstOrDefault();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		public async Task StoreRelatedTopics(SubTopics subTopics)
		{
			try
			{
				RedisKey key = $"RelatedTopics:{subTopics.TopicName}";
				await _redis.Database(2).SetAdd(key, RedisKeys.HashtagGrowKeys.SearchSession, JsonConvert.SerializeObject(subTopics), TimeSpan.FromDays(600));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}

using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache
{
	public class HashtagCoprusCache : IHashtagCoprusCache
	{
		private readonly IRedisClient _redisClient;
		public HashtagCoprusCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}
		public async Task AddHashtags(IEnumerable<HashtagsModel> hashtags)
		{
			await WithExceptionLogAsync(async () =>
			{
				var seperateByLanguageAndTopic = hashtags.GroupBy(_ => new { _.Language, _.Topic }).Where(_ => _ != null);
				foreach (var item in seperateByLanguageAndTopic)
				{
					if (string.IsNullOrEmpty(item.Key.Topic) || string.IsNullOrEmpty(item.Key.Language)) continue;
					else
					{
						string uniqueId = "Hashtags:" + item.Key.Topic + ":" + item.Key.Language;
						if (!string.IsNullOrEmpty(uniqueId))
						{
							await _redisClient.Database(0).SetAdd(uniqueId, RedisKeys.HashtagGrowKeys.Corpus, JsonConvert.SerializeObject(item), TimeSpan.FromDays(9999));
						}
					}
				}
			});
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtags(string topic, string lang, int limit)
		{
			IEnumerable<HashtagsModel> response = null;
			string uniqueId = "Hashtags:" + topic + ":" + lang;
			await WithExceptionLogAsync(async () =>
			{
				if (uniqueId != null)
				{
					response = await _redisClient.Database(0).GetMembers<HashtagsModel>(uniqueId, RedisKeys.HashtagGrowKeys.Corpus);
				}
			});
			return limit > 0 ? response.Take(limit) : response;
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

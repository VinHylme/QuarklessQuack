using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache
{
	public class MediaCorpusCache : IMediaCorpusCache
	{
		private readonly IRedisClient _redisClient;
		public MediaCorpusCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}
		public async Task AddMedias(IEnumerable<MediaCorpus> medias)
		{
			await WithExceptionLogAsync(async () =>
			{
				var seperateByLanguageAndTopic = medias.Where(l=>!string.IsNullOrEmpty(l.Language) && !string.IsNullOrEmpty(l.Topic))
				.GroupBy(_ => new { _.Language, _.Topic }).Where(_ => _ != null);
				foreach (var item in seperateByLanguageAndTopic)
				{
					if (string.IsNullOrEmpty(item.Key.Topic) || string.IsNullOrEmpty(item.Key.Language)) continue;
					else
					{
						string uniqueId = "Media:" + item.Key.Topic + ":" + item.Key.Language;
						if (!string.IsNullOrEmpty(uniqueId))
						{
							await _redisClient.Database(0).SetAdd(uniqueId, RedisKeys.HashtagGrowKeys.Corpus, JsonConvert.SerializeObject(item), TimeSpan.FromDays(9999));
						}
					}
				}
			});
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, int limit)
		{
			IEnumerable<MediaCorpus> response = null;
			string uniqueId = "Media:" + topic + ":" + lang;
			await WithExceptionLogAsync(async () =>
			{
				if (uniqueId != null)
				{
					var medobj = await _redisClient.Database(0).GetMembers<object>(uniqueId, RedisKeys.HashtagGrowKeys.Corpus);
					if (medobj != null)
						response = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<MediaCorpus>>>(JsonConvert.SerializeObject(medobj)).SquashMe();

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

using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuarklessContexts.Extensions;
namespace QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache
{
	public class CommentCorpusCache : ICommentCorpusCache
	{
		private readonly IRedisClient _redisClient;
		public CommentCorpusCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;

		}

		public async Task AddComments(IEnumerable<CommentCorpus> comments)
		{
			await WithExceptionLogAsync(async () =>
			{
				var seperateByLanguageAndTopic = comments.GroupBy(_ => new { _.Language, _.From.TopicRequest }).Where(_ => _ != null);
				foreach (var item in seperateByLanguageAndTopic)
				{
					if (item.Key.TopicRequest == null|| string.IsNullOrEmpty(item.Key.Language)) continue;
					else
					{
						string uniqueId = "Comments:" + item.Key.TopicRequest.Name + ":" + item.Key.Language;
						if (!string.IsNullOrEmpty(uniqueId))
						{
							await _redisClient.Database(0).SetAdd(uniqueId, RedisKeys.HashtagGrowKeys.Corpus, JsonConvert.SerializeObject(item), TimeSpan.FromDays(9999));
						}
					}
				}
			});
		}
		public async Task<IEnumerable<CommentCorpus>> GetComments(string topic, string lang, int limit)
		{
			IEnumerable<CommentCorpus> response = null;
			string uniqueId = "Comments:" + topic + ":" + lang;
			await WithExceptionLogAsync(async () =>
			{
				if (uniqueId != null)
				{
					var comobj = await _redisClient.Database(0).GetMembers<object>(uniqueId, RedisKeys.HashtagGrowKeys.Corpus);
					if(comobj!=null)
						response = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<CommentCorpus>>>(JsonConvert.SerializeObject(comobj)).SquashMe();
				}
			});
			return limit > 0 ? response.Take(limit) :response;
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

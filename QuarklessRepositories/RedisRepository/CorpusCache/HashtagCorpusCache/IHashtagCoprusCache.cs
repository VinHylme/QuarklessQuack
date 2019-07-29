using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache
{
	public interface IHashtagCoprusCache
	{
		Task AddHashtags(IEnumerable<HashtagsModel> hashtags);
		Task<IEnumerable<HashtagsModel>> GetHashtags(string topic, string lang, int limit);
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache
{
	public interface IMediaCorpusCache
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, int limit);
	}
}
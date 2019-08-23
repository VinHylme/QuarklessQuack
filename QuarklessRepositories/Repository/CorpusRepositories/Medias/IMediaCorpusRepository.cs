using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessRepositories.Repository.CorpusRepositories.Medias
{
	public interface IMediaCorpusRepository
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(IEnumerable<FilterDefinition<MediaCorpus>> searchRepository = null, int limit = -1);
		Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string language = null,string mapedLang = null, int limit = -1);
		Task<long> GetMediasCount(string topic);
		Task UpdateTopicName(string topic, string newTopic);
	}
}
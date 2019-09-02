using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessLogic.ServicesLogic.CorpusLogic
{
	public interface IMediaCorpusLogic
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, string langmapped, int limit);
		Task<long> MediasCount(string topic);
		Task UpdateTopicName(string topic, string newTopic);
		Task UpdateAllMediasLanguagesToLower();
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessLogic.ServicesLogic.CorpusLogic
{
	public interface IMediaCorpusLogic
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(int topicHash, int limit = -1, bool skip = true);
		//Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, int limit = -1, bool skip = true);
		Task<long> MediasCount(string topic);
		Task AddMedia(MediaCorpus mediaCorpus);
	}
}
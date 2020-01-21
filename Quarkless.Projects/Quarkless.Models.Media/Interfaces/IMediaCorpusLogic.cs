using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Media.Interfaces
{
	public interface IMediaCorpusLogic
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(int topicHash, int limit = -1, bool skip = true);
		Task<long> MediasCount(string topic);
		Task AddMedia(MediaCorpus mediaCorpus);
	}
}
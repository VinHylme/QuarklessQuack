using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.Media.Models.Interfaces
{
	public interface IMediaCorpusRepository
	{
		Task AddMedias(IEnumerable<MediaCorpus> medias);
		Task<IEnumerable<MediaCorpus>> GetMedias(int topicHashCode, int limit = -1, bool skip = true);
		Task<long> GetMediasCount(string topic);
		Task AddMedia(MediaCorpus mediaCorpus);
	}
}
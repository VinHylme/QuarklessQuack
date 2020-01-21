using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Hashtag.Interfaces
{
	public interface IHashtagsRepository
	{
		Task<bool> AddHashtags(IEnumerable<HashtagsModel> hashtags);
		Task<bool> RemoveHashtags(IEnumerable<string> hashtag_ids);
		Task<IEnumerable<HashtagsModel>> GetHashtags(int topicHashCode, int limit = -1, bool skip = true);
		Task<List<HashtagsModel>> GetHashtagsByTopic(string topic, int limit);
	}
}

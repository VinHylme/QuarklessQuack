using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Handlers.HashtagBuilder
{
	public interface IHashtagGenerator
	{
		Task<List<string>> GetHashtagsFromMediaSearch(string topic, int limit);

		Task<List<string>> SuggestHashtags(Topic profileTopic = null, CTopic mediaTopic = null, IEnumerable<string> images = null,
			int pickAmount = 20, int keywordsFetchAmount = 4,
			IEnumerable<string> preDefinedHashtagsToUse = null, int retries = 3);
	}
}
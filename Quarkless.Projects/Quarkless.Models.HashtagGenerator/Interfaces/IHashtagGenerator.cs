using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Topic;

namespace Quarkless.Models.HashtagGenerator.Interfaces
{
	public interface IHashtagGenerator
	{
		Task<List<HashtagResponse>> GetHashtagsFromMediaSearch(string topic, int limit);
		Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtags(Source source,
			bool deepDive = false, bool includeMediaExample = true);

		Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtagsFromImages(
			bool deepDive = false, bool includeMediaExample = true, params string[] images);
		Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtagsFromImages(
			List<byte[]> images, bool deepDive = false, bool includeMediaExample = true);

		// Task<List<string>> SuggestHashtags(Profile.Topic profileTopic = null, CTopic mediaTopic = null, IEnumerable<string> images = null,
		// 	int pickAmount = 20, int keywordsFetchAmount = 4,
		// 	IEnumerable<string> preDefinedHashtagsToUse = null, int retries = 3);
		//
		// Task<List<string>> SuggestHashtags(Profile.Topic profileTopic = null, CTopic mediaTopic = null,
		// 	IEnumerable<byte[]> images = null, int pickAmount = 20, int keywordsFetchAmount = 4,
		// 	IEnumerable<string> preDefinedHashtagsToUse = null, int retries = 3);
	}
}
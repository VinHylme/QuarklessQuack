using InstagramApiSharp.Classes.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Topic.Interfaces
{
	public interface ITopicLookupLogic
	{
		Task<List<InstaMedia>> GetMediasFromTopic(string topic, int limit);
		Task<List<HashtagResponse>> BuildRelatedTopics(CTopic topic, bool saveToDb,
			bool includeGoogleSuggest = true, int instagramTopicTakeAmount = 25);
		Task<TopicResponse> AddTopic(CTopic topic, bool includeGoogleSuggest = true, bool saveTopicsSuggested = false);
		Task<List<string>> AddTopics(List<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<List<CTopic>> GetHighestParents(CTopic topic);
		Task<CTopic> GetHighestParent(CTopic topic);
		Task<List<CTopic>> GetTopicsByParentId(string parentId, bool buildIfNotExists = false);
		Task<List<CTopic>> GetTopicByNameLike(string name);
		Task<List<CTopic>> GetTopicByName(string name);
		Task<List<CTopic>> GetCategories();
		Task<List<CTopic>> GetAllTopics();
		Task<long> DeleteAll();
		Task<long> DeleteAll(params string[] topicsId);
		Task ResetTopics();
	}
}

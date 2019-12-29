using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Logic.TopicLookupLogic
{
	public interface ITopicLookupLogic
	{
		Task<List<InstaMedia>> GetMediasFromTopic(string topic, int limit);
		Task<List<string>> BuildRelatedTopics(CTopic topic,bool saveToDb, bool includeGoogleSuggest = true);
		Task<TopicResponse> AddTopic(CTopic topic, bool includeGoogleSuggest = true);
		Task<List<string>> AddTopics(List<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<List<CTopic>> GetHighestParents(CTopic topic);
		Task<CTopic> GetHighestParent(CTopic topic);
		Task<List<CTopic>> GetTopicsByParentId(string parentId);
		Task<List<CTopic>> GetTopicByNameLike(string name);
		Task<List<CTopic>> GetTopicByName(string name);
		Task<List<CTopic>> GetCategories();
		Task<long> DeleteAll();
	}
}
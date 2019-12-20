using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Logic.TopicLookupLogic
{
	public interface ITopicLookupLogic
	{
		Task<string> AddTopic(CTopic topic);
		Task<List<string>> AddTopics(List<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<List<CTopic>> GetTopicByParentId(string parentId);
		Task<List<CTopic>> GetTopicByNameLike(string name);
		Task<List<CTopic>> GetTopicByName(string name);
		Task<List<CTopic>> GetCategories();
		Task<long> DeleteAll();
	}
}
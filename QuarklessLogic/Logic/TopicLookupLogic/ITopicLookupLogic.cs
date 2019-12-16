using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Logic.TopicLookupLogic
{
	public interface ITopicLookupLogic
	{
		Task<string> AddTopic(CTopic topic);
		Task<IEnumerable<string>> AddTopics(IEnumerable<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<IEnumerable<CTopic>> GetTopicByParentId(string parentId);
		Task<IEnumerable<CTopic>> GetTopicByNameLike(string name);
		Task<IEnumerable<CTopic>> GetTopicByName(string name);
		Task<IEnumerable<CTopic>> GetCategories();
		Task<long> DeleteAll();
	}
}
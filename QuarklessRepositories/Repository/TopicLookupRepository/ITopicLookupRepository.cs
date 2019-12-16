using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessRepositories.Repository.TopicLookupRepository
{
	public interface ITopicLookupRepository
	{
		Task<string> AddTopic(CTopic topic);
		Task<IEnumerable<string>> AddTopics(IEnumerable<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<IEnumerable<CTopic>> GetTopicsByParentId(string parentId);
		Task<IEnumerable<CTopic>> GetTopicsNameLike(string name);
		Task<IEnumerable<CTopic>> GetTopicsName(string name);
		Task<IEnumerable<CTopic>> GetCategories();
		Task<long> DeleteAll();
	}
}
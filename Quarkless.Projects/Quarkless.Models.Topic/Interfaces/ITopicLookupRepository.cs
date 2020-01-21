using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Topic.Interfaces
{
	public interface ITopicLookupRepository
	{
		Task<AddTopicResponse> AddTopic(CTopic topic);
		Task<List<string>> AddTopics(List<CTopic> topics);
		Task<CTopic> GetTopicById(string id);
		Task<List<CTopic>> GetTopicsByParentId(string parentId);
		Task<List<CTopic>> GetTopicsNameLike(string name);
		Task<List<CTopic>> GetTopicsName(string name);
		Task<List<CTopic>> GetCategories();
		Task<long> DeleteAll();
	}
}

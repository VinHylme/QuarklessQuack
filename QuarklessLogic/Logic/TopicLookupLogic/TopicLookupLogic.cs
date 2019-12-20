using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.Repository.TopicLookupRepository;

namespace QuarklessLogic.Logic.TopicLookupLogic
{
	public class TopicLookupLogic : ITopicLookupLogic
	{
		private readonly ITopicLookupRepository _topicLookupRepository;
		public TopicLookupLogic(ITopicLookupRepository repository) => _topicLookupRepository = repository;

		public async Task<string> AddTopic(CTopic topic) 
			=> await _topicLookupRepository.AddTopic(topic);
		public async Task<List<string>> AddTopics(List<CTopic> topics) 
			=> await _topicLookupRepository.AddTopics(topics);

		public async Task<CTopic> GetTopicById(string id) 
			=> await _topicLookupRepository.GetTopicById(id);

		public async Task<List<CTopic>> GetTopicByParentId(string parentId) 
			=> await _topicLookupRepository.GetTopicsByParentId(parentId);

		public async Task<List<CTopic>> GetTopicByNameLike(string name)
			=> await _topicLookupRepository.GetTopicsNameLike(name);
		public async Task<List<CTopic>> GetTopicByName(string name)
			=> await _topicLookupRepository.GetTopicsName(name);

		public async Task<List<CTopic>> GetCategories()
			=> await _topicLookupRepository.GetCategories();

		public async Task<long> DeleteAll()
			=> await _topicLookupRepository.DeleteAll();

	}
}

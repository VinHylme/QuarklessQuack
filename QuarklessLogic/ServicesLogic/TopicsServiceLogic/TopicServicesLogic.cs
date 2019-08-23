using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.RedisRepository.SearchCache;
using QuarklessRepositories.Repository.CorpusRepositories.Topic;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SubTopics = QuarklessContexts.Models.ServicesModels.DatabaseModels.SubTopics;

namespace QuarklessLogic.ServicesLogic
{
	public class TopicServicesLogic : ITopicServicesLogic
	{
		private readonly ITopicsRepository _topicsRepository;
		private readonly ITopicCategoryRepository _topicCategoryRepository;
		private readonly ISearchingCache _searchingCache;
		public TopicServicesLogic(ITopicsRepository topicsRepository, ITopicCategoryRepository topicCategoryRepository, ISearchingCache searchingCache)
		{
			_topicsRepository = topicsRepository;
			_topicCategoryRepository = topicCategoryRepository;
			_searchingCache = searchingCache;
		}
		public async Task AddRelated( QuarklessContexts.Models.Profiles.SubTopics subTopics)
		{
			await _searchingCache.StoreRelatedTopics(subTopics);
		}

		public async Task<QuarklessContexts.Models.Profiles.SubTopics> GetAllRelatedTopics(string topic)
		{
			return await _searchingCache.GetReleatedTopic(topic);
		}
		public async Task<bool> AddOrUpdateTopic(TopicsModel topics)
		{
			return await _topicsRepository.AddOrUpdateTopic(topics);
		}
		public async Task AddTopicCategories(IEnumerable<TopicCategories> topicCategories) => await _topicCategoryRepository.AddCategories(topicCategories);
		public async Task<IEnumerable<TopicCategories>> GetAllTopicCategories(){
			return await _topicCategoryRepository.GetAllCategories();
		}
		public async Task<TopicsModel> GetTopicByName(string topicName)
		{
			return await _topicsRepository.GetTopicByName(topicName);
		}
		public async Task<IEnumerable<TopicsModel>> GetTopics()
		{
			return await _topicsRepository.GetTopics();
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.RedisRepository.SearchCache;
using QuarklessRepositories.Repository.CorpusRepositories.Topic;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;

namespace QuarklessLogic.ServicesLogic.TopicsServiceLogic
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
		public async Task<IEnumerable<TopicCategories>> GetAllTopicCategories()
		{
			var allCategories = await _topicCategoryRepository.GetAllCategories();
			var uniqueByCat = allCategories.DistinctBy(x => x.CategoryName).DistinctBy(x => x.SubCategories);
			//var total = (await GetTopics())
			//	.Select(s => s.SubTopics)
			//	.SquashMe()
			//	.Where(y=>y.RelatedTopics.Count>0)
			//	.Distinct()
			//	.ToList();
			//var selectedCategories = new List<TopicCategories>();
			//foreach (var topicCategories in uniqueByCat)
			//{
			//	if (total.Any(c => topicCategories.SubCategories.Contains(c.Topic)))
			//		selectedCategories.Add(topicCategories);
			//}

			return uniqueByCat;
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

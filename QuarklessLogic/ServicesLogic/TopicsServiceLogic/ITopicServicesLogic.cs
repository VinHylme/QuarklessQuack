using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.ServicesLogic
{
	public interface ITopicServicesLogic
	{
		Task<bool> AddOrUpdateTopic(TopicsModel topics);
		Task<TopicsModel> GetTopicByName(string topicName);
		Task<IEnumerable<TopicsModel>> GetTopics();
		Task AddTopicCategories(IEnumerable<TopicCategory> topicCategories);
		Task<IEnumerable<TopicCategory>> GetAllTopicCategories();
		Task AddRelated(QuarklessContexts.Models.Profiles.SubTopics subTopics);
		Task<QuarklessContexts.Models.Profiles.SubTopics> GetAllRelatedTopics(string topic);
	}
}
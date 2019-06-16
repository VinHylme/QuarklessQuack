using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository
{
	public interface ITopicsRepository
	{
		Task<bool> AddOrUpdateTopic(TopicsModel topics);
		Task<TopicsModel> GetTopicByName(string topicName);
		Task<IEnumerable<TopicsModel>> GetTopics();
	}
}
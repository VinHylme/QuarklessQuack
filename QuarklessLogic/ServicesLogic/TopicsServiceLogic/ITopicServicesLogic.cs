using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessLogic.ServicesLogic
{
	public interface ITopicServicesLogic
	{
		Task<bool> AddOrUpdateTopic(TopicsModel topics);
		Task<TopicsModel> GetTopicByName(string topicName);
		Task<IEnumerable<TopicsModel>> GetTopics();
	}
}
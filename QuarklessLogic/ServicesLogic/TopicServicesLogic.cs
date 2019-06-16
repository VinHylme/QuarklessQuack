using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.ServicesLogic
{
	public class TopicServicesLogic : ITopicServicesLogic
	{
		private readonly ITopicsRepository _topicsRepository;
		public TopicServicesLogic(ITopicsRepository topicsRepository)
		{
			_topicsRepository = topicsRepository;
		}

		public async Task<bool> AddOrUpdateTopic(TopicsModel topics)
		{
			return await _topicsRepository.AddOrUpdateTopic(topics);
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

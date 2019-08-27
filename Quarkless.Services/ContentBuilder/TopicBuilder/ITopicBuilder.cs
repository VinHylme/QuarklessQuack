using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessContexts.Models.Topics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services.ContentBuilder.TopicBuilder
{
	public interface ITopicBuilder
	{
		void Init(UserStoreDetails user);
		Task<Topics> BuildTopics(ProfileModel profile, int takeSuggested = 15);
		Task<TopicsModel> Build(string topic, int takeSuggested = -1, int takeHowMany = -1);
		Task<IEnumerable<string>> BuildHashtags(string topic,string subcategory, string language, int limit = 1, int pickRate = 20);
		Task AddTopicCategories(IEnumerable<TopicCategories> topicCategories);
		Task<IEnumerable<TopicCategories>> GetAllTopicCategories();
		Task BuildTopics(IEnumerable<TopicCategories> topicCategories);
		Task<QuarklessContexts.Models.Profiles.SubTopics> GetAllRelatedTopics(string topic);
		Task Update(string selected, string subItem);
		Task<IEnumerable<TopicsModel>> GetTopics();
	}
}
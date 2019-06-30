using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services.ContentBuilder.TopicBuilder
{
	public interface ITopicBuilder
	{
		void Init(IUserStoreDetails user);
		Task<TopicsModel> Build(string topic, int takeSuggested = -1, int takeHowMany = -1);
		Task<IEnumerable<string>> BuildHashtags(string topic, int limit = 1, int pickRate = 20);
	}
}
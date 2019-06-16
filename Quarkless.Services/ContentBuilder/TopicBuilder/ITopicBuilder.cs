using Quarkless.Queue.Jobs.JobOptions;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services.ContentBuilder.TopicBuilder
{
	public interface ITopicBuilder
	{
		void Init(UserStore user);
		Task<TopicsModel> Build(string topic, int takeHowMany = 8);
		Task<IEnumerable<string>> BuildHashtags(string topic, int limit = 1, int pickRate = 20);
	}
}
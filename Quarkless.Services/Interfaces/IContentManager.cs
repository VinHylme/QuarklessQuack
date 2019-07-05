using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services.Interfaces
{
	public interface IContentManager
	{
		string GenerateComment(string topic, string language);
		string GenerateMediaInfo(TopicsModel topicSelect, string language);
		string GenerateText(string topic, string lang, int type, int limit, int size);
		Task<IEnumerable<string>> GetHashTags(string topic, int limit, int pickAmount);
		Task<List<TopicsModel>> GetTopics(List<string> topic, int takeSuggested = -1 , int limit = -1);
		Task<TopicsModel> GetTopic(string topic, int takeSuggested = -1, int limit = -1);

	}
}

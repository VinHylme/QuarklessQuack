using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels.Settings;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.QueryLogic
{
	public interface IQueryLogic
	{
		Task<ProfileConfiguration> GetProfileConfig();
		object SearchPlaces(string query);
		object AutoCompleteSearchPlaces(string query, int radius = 500);
		Task<Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null);
		Task<SubTopics> GetReleatedKeywords(string topicName);
	}
}
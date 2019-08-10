using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;

namespace QuarklessRepositories.RedisRepository.SearchCache
{
	public interface ISearchingCache
	{
		Task<SearchRequest> GetSearchData(string userId, SearchRequest find, string instagramId = null, string profileId = null);
		Task StoreSearchData(string userId, SearchRequest search, string instagramId = null, string profileId = null);
		Task<SubTopics> GetReleatedTopic(string topic);
		Task StoreRelatedTopics(SubTopics subTopics);
	}
}
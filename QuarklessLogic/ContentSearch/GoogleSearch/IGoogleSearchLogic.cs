using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessLogic.ContentSearch.GoogleSearch
{
	public interface IGoogleSearchLogic
	{
		IGoogleSearchLogic WithProxy(ProxyModel proxy = null);
		Task<List<string>> GetSuggestions(string query);
		SearchResponse<Media> SearchViaGoogle(SearchImageModel searchImageQuery);
		SearchResponse<Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit,
			int offset = 0);
	}
}

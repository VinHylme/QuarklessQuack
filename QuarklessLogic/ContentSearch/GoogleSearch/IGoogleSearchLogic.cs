using System.Collections.Generic;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessLogic.ContentSearch.GoogleSearch
{
	public interface IGoogleSearchLogic
	{
		void WithProxy(ProxyModel proxy);
		SearchResponse<Media> SearchViaGoogle(SearchImageModel searchImageQuery);
		SearchResponse<Media> SearchSimilarImagesViaGoogle(List<GroupImagesAlike> groupImages, int limit,
			int offset = 0);
	}
}

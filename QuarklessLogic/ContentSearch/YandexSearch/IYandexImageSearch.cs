using System.Collections.Generic;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessLogic.ContentSearch.YandexSearch
{
	public interface IYandexImageSearch
	{
		IYandexImageSearch WithProxy(ProxyModel proxy = null);
		SearchResponse<Media> SearchQueryRest(YandexSearchQuery yandexSearchQuery, int limit = 16);
		SearchResponse<Media> SearchSafeButSlow(IEnumerable<GroupImagesAlike> similarImages, int limit);
		SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages, int offset = 0);
		SearchResponse<Media> SearchRelatedImagesRest(IEnumerable<GroupImagesAlike> similarImages, 
			int numberOfPages, int offset = 0);
		Media Search(IEnumerable<GroupImagesAlike> similarImages, int limit);
	}
}
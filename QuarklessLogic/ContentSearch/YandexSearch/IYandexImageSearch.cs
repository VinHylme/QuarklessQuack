using System.Collections.Generic;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessLogic.ContentSearch.YandexSearch
{
	public interface IYandexImageSearch
	{
		void WithProxy(ProxyModel proxy);
		SearchResponse<Media> SearchQueryREST(YandexSearchQuery yandexSearchQuery, int limit = 16);
		SearchResponse<Media> SearchSafeButSlow(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit);
		SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages, int offset = 0);
		SearchResponse<Media> SearchRelatedImagesREST(IEnumerable<GroupImagesAlike> imagesAlikes, int numberOfPages, int offset = 0);
		Media Search(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit);
	}
}
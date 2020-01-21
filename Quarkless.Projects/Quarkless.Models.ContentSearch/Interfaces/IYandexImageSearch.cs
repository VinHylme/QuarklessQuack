using System.Collections.Generic;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.ContentSearch.Models.Yandex;
using Quarkless.Models.Proxy;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Models.ContentSearch.Interfaces
{
	public interface IYandexImageSearch
	{
		IYandexImageSearch WithProxy(ProxyModel proxy = null);
		SearchResponse<Media> SearchQueryRest(YandexSearchQuery yandexSearchQuery, int limit = 16);
		SearchResponse<Media> SearchSafeButSlow(IEnumerable<GroupImagesAlike> similarImages, int limit);
		SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages, int offset = 0);
		SearchResponse<Media> SearchRelatedImagesRest(IEnumerable<GroupImagesAlike> similarImages,
			int numberOfPages, int offset = 0);
	}
}

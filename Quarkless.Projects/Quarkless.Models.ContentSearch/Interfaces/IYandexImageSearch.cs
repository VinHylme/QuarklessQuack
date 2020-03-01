using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.ContentSearch.Models.Yandex;
using Quarkless.Models.Proxy;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Models.ContentSearch.Interfaces
{
	public interface IYandexImageSearch
	{
		IYandexImageSearch WithProxy(ProxyModel proxy = null);
		Task<SearchResponse<Media>> QueryImages(YandexSearchQuery query, int pageLimit = 1);
		Task<SearchResponse<Media>> QuerySimilarImages(IEnumerable<GroupImagesAlike> similarImages,
			int pageLimit);
	}
}

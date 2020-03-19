using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.ContentSearch.Models.Models.Yandex;
using Quarkless.Base.Proxy.Models;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.ContentSearch.Models.Interfaces
{
	public interface IYandexImageSearch
	{
		IYandexImageSearch WithProxy(ProxyModel proxy = null);
		Task<SearchResponse<Quarkless.Models.SearchResponse.Media>> QueryImages(YandexSearchQuery query, int pageLimit = 1);
		Task<SearchResponse<Quarkless.Models.SearchResponse.Media>> QuerySimilarImages(IEnumerable<GroupImagesAlike> similarImages,
			int pageLimit);
	}
}

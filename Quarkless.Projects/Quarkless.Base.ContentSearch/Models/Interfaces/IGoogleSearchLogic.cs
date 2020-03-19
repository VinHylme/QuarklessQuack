using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Proxy.Models;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.Common.Models.Topic;
using Quarkless.Models.SearchResponse;
using SearchGoogleImageRequestModel = Quarkless.Base.ContentSearch.Models.Models.SearchGoogleImageRequestModel;

namespace Quarkless.Base.ContentSearch.Models.Interfaces
{
	public interface IGoogleSearchLogic
	{
		IGoogleSearchLogic WithProxy(ProxyModel proxy = null);
		Task<List<string>> GetSuggestions(string query);
		Task<SearchResponse<Quarkless.Models.SearchResponse.Media>> SearchGoogleImages(CTopic topic, SearchGoogleImageRequestModel searchQuery);
		SearchResponse<Quarkless.Models.SearchResponse.Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit, int offset = 0);
	}
}

using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.Proxy;
using Quarkless.Models.SearchResponse;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.Topic;

namespace Quarkless.Models.ContentSearch.Interfaces
{
	public interface IGoogleSearchLogic
	{
		IGoogleSearchLogic WithProxy(ProxyModel proxy = null);
		Task<List<string>> GetSuggestions(string query);
		SearchResponse<Media> SearchViaGoogle(CTopic topic, SearchImageModel searchImageQuery);
		SearchResponse<Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit, int offset = 0);
	}
}

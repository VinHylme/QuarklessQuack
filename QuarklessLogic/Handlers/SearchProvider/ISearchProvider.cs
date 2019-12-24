using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.InstagramSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;

namespace QuarklessLogic.Handlers.SearchProvider
{
	public interface ISearchProvider
	{
		IInstagramContentSearch InstagramSearch
			(IAPIClientContainer client, ProxyModel proxy);
		IGoogleSearchLogic GoogleSearch { get; }
		IYandexImageSearch YandexSearch { get; }
	}
}
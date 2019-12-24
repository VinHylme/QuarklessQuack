using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.InstagramSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.ResponseLogic;

namespace QuarklessLogic.Handlers.SearchProvider
{
	public class SearchProvider : ISearchProvider
	{
		private readonly IResponseResolver _responseResolver;
		public SearchProvider(IResponseResolver responseResolver, IGoogleSearchLogic googleSearch, 
			IYandexImageSearch yandexSearch)
		{
			_responseResolver = responseResolver;
			GoogleSearch = googleSearch;
			YandexSearch = yandexSearch;
		}
		public IInstagramContentSearch InstagramSearch
			(IAPIClientContainer client, ProxyModel proxy) => new InstagramContentSearch(client, _responseResolver, proxy);

		public IGoogleSearchLogic GoogleSearch { get; }
		public IYandexImageSearch YandexSearch { get; }

	}
}

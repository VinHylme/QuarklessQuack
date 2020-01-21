using Quarkless.Logic.InstagramSearch;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.InstagramSearch.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Base.ContentSearch
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

		public IInstagramContentSearch InstagramSearch(IApiClientContainer client, ProxyModel proxy) 
			=> new InstagramContentSearch(client, _responseResolver, proxy);
		public IGoogleSearchLogic GoogleSearch { get; }
		public IYandexImageSearch YandexSearch { get; }
	}
}

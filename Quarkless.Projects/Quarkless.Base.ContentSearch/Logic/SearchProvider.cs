using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramSearch.Logic;
using Quarkless.Base.InstagramSearch.Models.Interfaces;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.ResponseResolver.Models.Interfaces;

namespace Quarkless.Base.ContentSearch.Logic
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

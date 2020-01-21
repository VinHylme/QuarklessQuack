using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.InstagramSearch.Interfaces;
using Quarkless.Models.Proxy;

namespace Quarkless.Base.ContentSearch
{
	public interface ISearchProvider
	{
		IInstagramContentSearch InstagramSearch
			(IApiClientContainer client, ProxyModel proxy);
		IGoogleSearchLogic GoogleSearch { get; }
		IYandexImageSearch YandexSearch { get; }
	}
}
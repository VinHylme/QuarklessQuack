using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramSearch.Models.Interfaces;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.ContentSearch.Models.Interfaces
{
	public interface ISearchProvider
	{
		IInstagramContentSearch InstagramSearch
			(IApiClientContainer client, ProxyModel proxy);
		IGoogleSearchLogic GoogleSearch { get; }
		IYandexImageSearch YandexSearch { get; }
	}
}
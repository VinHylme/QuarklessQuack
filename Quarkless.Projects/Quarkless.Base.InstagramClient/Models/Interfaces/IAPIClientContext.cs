using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Models.Interfaces
{
	public interface IApiClientContext
	{
		Task<InstagramAccountFetcherResponse> Create(string userId, string instaId);
		IInstaClient EmptyClient {get; }
		IInstaClient EmptyClientWithUser(UserSessionData userData);
		IInstaClient EmptyClientWithProxy(ProxyModel model, bool genDevice = false);
	}
}
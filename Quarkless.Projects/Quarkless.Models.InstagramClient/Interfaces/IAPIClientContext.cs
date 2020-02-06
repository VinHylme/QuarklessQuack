using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient.Interfaces
{
	public interface IApiClientContext
	{
		Task<InstagramAccountFetcherResponse> Create(string userId, string instaId);
		IInstaClient EmptyClient {get; }
		IInstaClient EmptyClientWithUser(UserSessionData userData);
		IInstaClient EmptyClientWithProxy(ProxyModel model, bool genDevice = false);
	}
}
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Models.Interfaces
{
	public interface IClientContextProvider
	{
		Task<InstagramAccountFetcherResponse> Get(string accId, string insAccId);
		IInstaClient InitialClientGenerate();
		IInstaClient InitialClientGenerate(UserSessionData userData);
		IInstaClient InitialClientGenerateWithProxy(ProxyModel model, bool genDevice = false);
	}
}
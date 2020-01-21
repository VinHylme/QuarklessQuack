using System.Threading.Tasks;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;

namespace Quarkless.Logic.InstagramClient
{
	public interface IClientContextProvider
	{
		Task<ContextContainer> Get(string accId, string insAccId);
		IInstaClient InitialClientGenerate();
		IInstaClient InitialClientGenerateWithProxy(ProxyModel model, bool genDevice = false);
	}
}
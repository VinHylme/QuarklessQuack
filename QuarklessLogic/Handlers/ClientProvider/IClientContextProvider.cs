using System.Threading.Tasks;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Proxies;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public interface IClientContextProvider
	{
		Task<ContextContainer> Get(string accId, string insAccId);
		InstaClient InitialClientGenerate() ;
		InstaClient InitialClientGenerateWithProxy(ProxyModel model);
	}
}
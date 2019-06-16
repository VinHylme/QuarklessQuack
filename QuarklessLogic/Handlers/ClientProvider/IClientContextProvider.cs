using System.Threading.Tasks;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public interface IClientContextProvider
	{
		Task<ContextContainer> Get(string accId, string insAccId);
		InstaClient InitialClientGenerate() ;
	}
}
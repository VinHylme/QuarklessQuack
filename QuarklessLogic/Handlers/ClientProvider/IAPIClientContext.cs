using System.Threading.Tasks;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Proxies;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public interface IAPIClientContext
	{
		Task<ContextContainer> Create(string userId, string instaId);
		InstaClient EmptyClient {get; }
		InstaClient EmptyClientWithProxy(ProxyModel model, bool genDevice = false);
	}
}
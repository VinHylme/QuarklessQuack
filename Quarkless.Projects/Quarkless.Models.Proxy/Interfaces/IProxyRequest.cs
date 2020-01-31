using System.Threading.Tasks;

namespace Quarkless.Models.Proxy.Interfaces
{
	public interface IProxyRequest
	{ 
		Task<bool> TestConnectivity(ProxyModel proxy);
		Task<ProxyModel> AssignProxy(string accountId, string instagramAccountId, string locationQuery);
	}
}
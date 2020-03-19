using System.Threading.Tasks;

namespace Quarkless.Base.Proxy.Models.Interfaces
{
	public interface IProxyRequest
	{ 
		Task<bool> TestConnectivity(ProxyModel proxy);
		Task<ProxyModel> AssignProxy(string accountId, string instagramAccountId, string locationQuery);
		Task<bool> UnAssignProxy(string instagramAccountId);
	}
}
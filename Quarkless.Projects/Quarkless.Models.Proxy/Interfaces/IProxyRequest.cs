using System.Threading.Tasks;

namespace Quarkless.Models.Proxy.Interfaces
{
	public interface IProxyRequest
	{
		Task<ProxyModel> AssignProxy(string accountId, string instagramAccountId, string locationQuery);
	}
}
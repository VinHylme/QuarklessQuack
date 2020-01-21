using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;

namespace Quarkless.Logic.Proxy
{
	public interface IProxyLogic
	{
		HttpClient GetHttpClient(ProxyModel proxy);
		Task<string> TestProxyConnectivity(ProxyModel proxy);
		Task<double?> TestSpeedOfProxy(ProxyModel proxy);
		Task<bool> AssignProxy(ProxyModel proxy);
		Task<ProxyModel> ReassignProxy(string proxyId, ProxyModel newModel);
		Task<bool> DeleteProxyAssigned(string proxyId);
		Task<ProxyModel> GetProxyAssigned(string accountId, string instagramAccountId);
		Task<ProxyModel> GetProxyAssigned(string instagramAccountId);
		Task<List<ProxyModel>> GetAllProxyAssigned();
		Task<List<ProxyModel>> GetAllProxyAssigned(ProxyType type);
		Task<List<ProxyModel>> GetAllProxyAssigned(string accountId);
	}
}
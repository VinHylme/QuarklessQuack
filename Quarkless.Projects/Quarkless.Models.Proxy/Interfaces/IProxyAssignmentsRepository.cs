using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Proxy.Enums;

namespace Quarkless.Models.Proxy.Interfaces
{
	public interface IProxyAssignmentsRepository
	{
		Task<bool> AddAssignedProxy(ProxyModel proxy);
		Task<ProxyModel> ReassignProxy(string proxyId, ProxyModel newModel);
		Task<bool> DeleteProxyAssigned(string proxyId);
		Task<ProxyModel> GetProxyAssigned(string accountId, string instagramAccountId);
		Task<ProxyModel> GetProxyAssigned(string instagramAccountId);
		Task<List<ProxyModel>> GetAllProxyAssigned(string accountId);
		Task<List<ProxyModel>> GetAllProxyAssigned(ProxyType type);
		Task<List<ProxyModel>> GetAllProxyAssigned();
	}
}

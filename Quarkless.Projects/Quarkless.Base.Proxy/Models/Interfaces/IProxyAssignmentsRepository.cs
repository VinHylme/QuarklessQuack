using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Proxy.Models.Enums;

namespace Quarkless.Base.Proxy.Models.Interfaces
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

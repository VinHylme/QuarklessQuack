using QuarklessContexts.Models.Proxies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.ProxyLogic
{
	public interface IProxyLogic
	{
		Task<bool> AssignProxy(AssignedTo assignedTo);
		Task<bool> RemoveUserFromProxy(AssignedTo assignedTo);
		bool AddProxy(ProxyModel proxy);
		bool AddProxies(List<ProxyModel> proxies);
		Task<IEnumerable<ProxyModel>> GetAllAssignedProxies();
		Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId);

		Task<bool> TestProxy(ProxyModel proxy);
	}
}
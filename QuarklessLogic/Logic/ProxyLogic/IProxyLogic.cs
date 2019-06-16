using QuarklessContexts.Models.Proxies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.ProxyLogic
{
	public interface IProxyLogic
	{
		Task<bool> AssignProxy(AssignedTo assignedTo);
		bool AddProxy(ProxyModel proxy);
		bool AddProxies(List<ProxyModel> proxies);
		bool TestProxy(ProxyModel proxy);
		Task<IEnumerable<ProxyModel>> GetAllAssignedProxies();
		Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId);
	}
}
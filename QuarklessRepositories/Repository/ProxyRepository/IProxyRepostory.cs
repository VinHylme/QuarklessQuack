using QuarklessContexts.Models.Proxies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.ProxyRepository
{
	public interface IProxyRepostory
	{
		void AddProxies(List<ProxyModel> proxies);
		void AddProxy(ProxyModel proxy);
		Task<bool> AssignProxy(AssignedTo assignedTo);
		Task<IEnumerable<ProxyModel>> GetAllAssignedProxies();
		Task<ProxyModel> GetAssignedProxyByInstaId(string instagramAccountId);
		Task<ProxyModel> GetAssignedProxyOf(string accountId, string instagramAccountId);
		Task<IEnumerable<ProxyModel>> GetAssignedProxyByAccountId(string accountId);
	}
}
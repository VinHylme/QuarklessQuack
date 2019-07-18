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
		Task<bool> TestProxy(ProxyItem proxy);
		Task<IEnumerable<ProxyModel>> GetAllAssignedProxies();
		Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId);
		Task<ProxyModel> RetrieveRandomProxy(bool get = true, bool post = true, bool cookies = true, bool referer = true,
			bool userAgent = true, int port = -1, string city = null, string state = null, string country = null,
			ConnectionType connectionType = ConnectionType.Any);
	}
}
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
		Task<ProxyModel> RetrieveRandomProxy(bool? get = null, bool? post = null, bool? cookies = null, bool? referer = null,
			bool? userAgent = null, int port = -1, string city = null, string state = null, string country = null,
			ConnectionType connectionType = ConnectionType.Any);
	}
}
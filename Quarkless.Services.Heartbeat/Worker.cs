using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Proxies;

namespace Quarkless.Services.Heartbeat
{
	public class Worker
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProxyModel ProxyModel { get; set; }
		public bool IsCurrentlyAssigned { get; set; }
	}
}
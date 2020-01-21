using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.Services.Heartbeat
{
	public class Worker
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProxyModel ProxyModel { get; set; }
	}
}
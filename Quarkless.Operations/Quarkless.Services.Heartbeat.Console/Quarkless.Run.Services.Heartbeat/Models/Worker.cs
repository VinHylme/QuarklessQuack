using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Run.Services.Heartbeat.Models
{
	public class Worker
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProxyModel ProxyModel { get; set; }
	}
}
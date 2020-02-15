using Quarkless.Models.Proxy;
using Location = Quarkless.Models.Common.Models.Location;

namespace Quarkless.Models.InstagramAccounts
{
	public class InstagramAccountPublishEventModel
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public string IpAddress { get; set; }
		public Location Location { get; set; }
		public ProxyModelShort UserProxy { get; set; }
	}
}
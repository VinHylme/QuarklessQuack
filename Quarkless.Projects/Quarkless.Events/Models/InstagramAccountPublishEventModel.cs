using Quarkless.Events.Models.PublishObjects;
using Quarkless.Models.Common.Models;

namespace Quarkless.Events.Models
{
	public class InstagramAccountPublishEventModel
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public string IpAddress { get; set; }
		public Location Location { get; set; }
		public ProxyModelShort UserProxy { get; set; }
	}
}
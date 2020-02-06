using Quarkless.Models.Common.Models;

namespace Quarkless.Models.InstagramAccounts
{
	public class InstagramAccountPublishEventModel
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public string IpAddress { get; set; }
		public Location Location { get; set; }
	}
}
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient
{
	public class InstagramClientAccount
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public ProxyModel Proxy { get; set; }
		public ProfileModel Profile { get; set; }
		public IInstaClient InstaClient { get; set; }
	}
}

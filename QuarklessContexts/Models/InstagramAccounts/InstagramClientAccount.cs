using QuarklessContexts.Models.Proxies;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class InstagramClientAccount
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public ProxyModel Proxy { get; set; }
		public Profiles.ProfileModel Profile { get; set; }
		public InstaClient.InstaClient InstaClient { get; set; }

	}
}

using InstagramApiSharp.API;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;

namespace QuarklessContexts.Models
{
	public class ContextContainer
	{
		public IInstaApi ActionClient { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel Proxy { get; set; }
		public ShortInstagramAccountModel InstagramAccount { get; set; }
	}
}

using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;

namespace Quarkless.Services.Heartbeat
{
	public class FullUserDetail
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel ProxyUsing { get; set; }
	}
}
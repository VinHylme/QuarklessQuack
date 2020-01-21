using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Profile;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.Services.Heartbeat
{
	public class FullUserDetail
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel ProxyUsing { get; set; }
	}
}
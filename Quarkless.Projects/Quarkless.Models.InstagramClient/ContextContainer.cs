using InstagramApiSharp.API;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Profile;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient
{
	public class ContextContainer
	{
		public IInstaApi ActionClient { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel Proxy { get; set; }
		public ShortInstagramAccountModel InstagramAccount { get; set; }
	}
}

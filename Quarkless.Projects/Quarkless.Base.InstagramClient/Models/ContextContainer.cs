using InstagramApiSharp.API;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Profile.Models;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Models
{
	public class ContextContainer
	{
		public IInstaApi ActionClient { get; set; }
		public IInstaClient InstaClient { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel Proxy { get; set; }
		public ShortInstagramAccountModel InstagramAccount { get; set; }
	}
}

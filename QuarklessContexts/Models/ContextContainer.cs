using InstagramApiSharp.API;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;

namespace QuarklessContexts.Models
{
	public class ContextContainer
	{
		public IInstaApi ActionClient { get; set; }
		public ProfileModel Profile { get; set; }
		public InstagramAccountModel InstagramAccount { get; set; }
	}
}

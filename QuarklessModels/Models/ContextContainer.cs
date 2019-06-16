using InstagramApiSharp.API;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Profiles;

namespace QuarklessModels.Models
{
	public class ContextContainer
	{
		public IInstaApi ActionClient { get; set; }
		public ProfileModel Profile { get; set; }
		public InstagramAccountModel InstagramAccount { get; set; }
	}
}

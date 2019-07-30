
using Newtonsoft.Json;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;

namespace QuarklessContexts.Models.Timeline
{
	public class UserStoreDetails
	{
		[JsonIgnore]
		public ProfileModel Profile { get; set; }
		[JsonIgnore]
		public ShortInstagramAccountModel shortInstagram { get; set; }

		public string OAccountId { get; set; }
		public string OAccessToken { get; set; }
		public string ORefreshToken { get; set; }
		public string OInstagramAccountUser { get; set; }
		public string OInstagramAccountUsername { get; set; }

		public void AddUpdateUser(string accountId, string InstagramAccountId, string accessToken)
		{
			this.OAccountId = accountId;
			this.OInstagramAccountUser = InstagramAccountId;
			this.OAccessToken = accessToken;
		}
	}
}

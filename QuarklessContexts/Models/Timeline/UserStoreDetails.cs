
namespace QuarklessContexts.Models.Timeline
{
	public class UserStoreDetails
	{
		public string OAccountId { get; set; }
		public string OAccessToken { get; set; }
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

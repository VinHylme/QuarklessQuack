
namespace QuarklessContexts.Models.Timeline
{
	public class UserStore
	{
		public string AccountId { get; set; }
		public string InstaAccountId { get; set; }
		public string AccessToken { get; set; }
		public UserStore(string accountId, string instaAccountId, string accessToken = null)
		{
			this.AccountId = accountId;
			this.InstaAccountId = instaAccountId;
			this.AccessToken = accessToken;
		}
	}
}

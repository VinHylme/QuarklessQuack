namespace QuarklessContexts.Models.Timeline
{
	public interface IUserStoreDetails
	{
		void AddUpdateUser(string accountId, string InstagramAccountId, string accessToken);
		string OAccountId { get; set; }
		string OAccessToken { get; set; }
		string OInstagramAccountUser { get; set; }
	}
}
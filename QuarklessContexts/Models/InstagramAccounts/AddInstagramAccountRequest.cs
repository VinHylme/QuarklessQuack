using InstagramApiSharp.Classes;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class AddInstagramAccountResponse
	{
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set; }
		public string ProfileId { get; set; }
	}
	public class SubmitVerifyCodeRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public InstaChallengeLoginInfo ChallangeLoginInfo { get; set; }
	}
	public class AddInstagramAccountRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string ComingFrom { get; set; }
		public int Type { get; set; }
	}
}

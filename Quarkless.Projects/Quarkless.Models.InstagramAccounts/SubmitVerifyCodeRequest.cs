using InstagramApiSharp.Classes;

namespace Quarkless.Models.InstagramAccounts
{
	public class SubmitVerifyCodeRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public InstaChallengeLoginInfo ChallengeLoginInfo { get; set; }
	}
}
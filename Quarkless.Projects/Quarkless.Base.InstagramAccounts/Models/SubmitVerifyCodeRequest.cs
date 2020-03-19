using InstagramApiSharp.Classes;

namespace Quarkless.Base.InstagramAccounts.Models
{
	public class SubmitVerifyCodeRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public InstaChallengeLoginInfo ChallengeLoginInfo { get; set; }
	}
}
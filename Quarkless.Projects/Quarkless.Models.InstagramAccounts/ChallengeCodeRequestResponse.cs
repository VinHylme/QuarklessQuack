using InstagramApiSharp.Classes;

namespace Quarkless.Models.InstagramAccounts
{
	public class ChallengeCodeRequestResponse
	{
		public string Verify { get; set;  }
		public string Details { get; set; }
		public InstaChallengeLoginInfo ChallengePath { get; set; }
	}
}
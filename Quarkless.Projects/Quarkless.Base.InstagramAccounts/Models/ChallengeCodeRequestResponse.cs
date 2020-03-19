using InstagramApiSharp.Classes;

namespace Quarkless.Base.InstagramAccounts.Models
{
	public class ChallengeCodeRequestResponse
	{
		public string Verify { get; set;  }
		public string Details { get; set; }
		public InstaChallengeLoginInfo ChallengePath { get; set; }
	}
}
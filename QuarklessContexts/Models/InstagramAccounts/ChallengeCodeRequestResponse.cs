using InstagramApiSharp.Classes;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class ChallengeCodeRequestResponse
	{
		public string Verify { get; set;  }
		public string Details { get; set; }
		public InstaChallengeLoginInfo ChallangePath { get; set; }
	}
}
using InstagramApiSharp.Classes;

namespace Quarkless.Models.InstagramAccounts
{
	public class SubmitChallengeResponse
	{
		public IResult<InstaLoginResult> Result { get; set; }
		public string InstagramId { get; set; }
	}
}

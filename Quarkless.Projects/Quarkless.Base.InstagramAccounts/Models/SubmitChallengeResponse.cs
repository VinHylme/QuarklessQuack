using InstagramApiSharp.Classes;

namespace Quarkless.Base.InstagramAccounts.Models
{
	public class SubmitChallengeResponse
	{
		public IResult<InstaLoginResult> Result { get; set; }
		public string InstagramId { get; set; }
	}
}

using InstagramApiSharp.Classes;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class SubmitChallengeResponse
	{
		public IResult<InstaLoginResult> Result { get; set; }
		public string InstagramId { get; set; }
	}
}

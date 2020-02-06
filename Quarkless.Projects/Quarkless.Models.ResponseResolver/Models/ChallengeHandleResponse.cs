using InstagramApiSharp.Classes;
using Quarkless.Models.ResponseResolver.Enums;

namespace Quarkless.Models.ResponseResolver.Models
{
	public class ChallengeHandleResponse
	{
		public ChallengeResponse ResponseCode { get; set; }
		public VerifyMethod VerifyMethod { get; set; } = VerifyMethod.None;
		public bool IsUserLoggedIn { get; set; }
		public ResultInfo RequestInfo { get; set; }
		public string StepDataEmailOrPhone { get; set; }
		public InstaChallengeLoginInfo ChallengeData { get; set; }

		public ChallengeHandleResponse(ChallengeResponse responseCode, ResultInfo requestInfo)
		{
			ResponseCode = responseCode;
			RequestInfo = requestInfo;
		}
	}
}

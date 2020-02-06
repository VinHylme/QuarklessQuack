using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.ResponseResolver.Enums;

namespace Quarkless.Models.ResponseResolver.Models
{
	public class VerifyDataResponse
	{
		public string Verify { get; set; } = VerifyMethod.None.GetDescription();
		public VerifyMethod VerifyMethod { get; set; } = VerifyMethod.None;
		public VerifyStatusCode StatusCode { get; set; } = VerifyStatusCode.Continue;
		public string VerifyDetail { get; set; }
		public InstaChallengeLoginInfo ChallengeData { get; set; }
		public bool Handled { get; set; } = false;
	}
}
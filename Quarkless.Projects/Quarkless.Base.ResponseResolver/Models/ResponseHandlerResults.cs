using InstagramApiSharp.Classes;

namespace Quarkless.Base.ResponseResolver.Models
{
	public class ResponseHandlerResults
	{
		public ChallengeHandleResponse ChallengeResponse { get; set; }
		public ResponseType ResponseType { get; set; }
		public bool Resolved { get; set; }
		public bool UpdatedAccountState { get; set; }
	}
}
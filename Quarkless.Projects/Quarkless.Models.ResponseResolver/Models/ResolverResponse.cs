using InstagramApiSharp.Classes;

namespace Quarkless.Models.ResponseResolver.Models
{
	public class ResolverResponse<TResult>
	{
		public ResponseHandlerResults HandlerResults { get; set; }
		public IResult<TResult> Response { get; set; }
	}
}
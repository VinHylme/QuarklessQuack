
namespace QuarklessContexts.Models.ResponseModels
{
	public enum ResponseCode
	{
		InternalServerError,
		CaptchaRequired,
		Success,
		Timeout,
		ReachedEndAndNull
	}
	public class SearchResponse<TResponse>
	{
		public TResponse Result { get; set; }
		public ResponseCode StatusCode { get; set; }
		public string Message { get; set; }
	}
}

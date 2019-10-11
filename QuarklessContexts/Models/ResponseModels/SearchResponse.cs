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
	public class SearchResponse<TResponse> where TResponse : new()
	{
		public TResponse Result { get; set; } = new TResponse();
		public ResponseCode StatusCode { get; set; }
		public string Message { get; set; }
	}
}

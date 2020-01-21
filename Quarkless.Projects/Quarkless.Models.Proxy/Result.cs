namespace Quarkless.Models.Proxy
{
	public class Result<TResponse>
	{
		public bool IsSuccess { get; set; }
		public TResponse Response { get; set; }
		public Error Error { get; set; }
	}
}
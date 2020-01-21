using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Common.Models.Search
{
	public class SearchResponse<TResponse> where TResponse : new()
	{
		public TResponse Result { get; set; } = new TResponse();
		public ResponseCode StatusCode { get; set; }
		public string Message { get; set; }
	}
}

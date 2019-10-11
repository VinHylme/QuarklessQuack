using System.Net;

namespace QuarklessContexts.Models
{
	public interface IResponseResults
	{
		int ErrorsCount { get; set; }
		string JobId { get; set; }
		HttpStatusCode StatusCode { get; set; }
		string Content { get; set; }

	}
}

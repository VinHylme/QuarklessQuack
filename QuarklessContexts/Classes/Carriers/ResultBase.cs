using System.Net;

namespace QuarklessContexts.Classes.Carriers
{
	public class ResultBase<T>
	{
		public string Message { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public T Results { get; set; }
	}
}

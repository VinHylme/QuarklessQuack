using System;
using System.Net;

namespace QuarklessContexts.Classes.Carriers
{
	public struct ErrorResponse
	{
		public string Message ;
		public HttpStatusCode StatusCode;
		public Exception Exception;
	}
	public class ResultCarrier<T>
	{
		public bool IsSuccesful { get; set; }
		public ErrorResponse Info {get; set;}
		public T Results { get; set; }
	}
}

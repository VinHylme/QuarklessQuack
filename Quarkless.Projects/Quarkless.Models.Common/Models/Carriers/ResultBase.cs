using System;
using System.Net;

namespace Quarkless.Models.Common.Models.Carriers
{
	public struct ErrorResponse
	{
		public string Message ;
		public HttpStatusCode StatusCode;
		public Exception Exception;
	}
	public class ResultCarrier<T>
	{
		public bool IsSuccessful { get; set; }
		public ErrorResponse Info {get; set;}
		public T Results { get; set; }
	}
}

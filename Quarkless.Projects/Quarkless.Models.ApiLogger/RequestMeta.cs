using System;

namespace Quarkless.Models.ApiLogger
{
	public class RequestMeta
	{
		public string RequestContentType { get; set; }
		public string RequestUri { get; set; }
		public string RequestMethod { get; set; }
		//public Stream RequestBody { get; set;  }
		public DateTime? RequestTimestamp { get; set; }
	}
}
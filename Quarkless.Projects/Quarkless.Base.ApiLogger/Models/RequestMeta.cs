using System;

namespace Quarkless.Base.ApiLogger.Models
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
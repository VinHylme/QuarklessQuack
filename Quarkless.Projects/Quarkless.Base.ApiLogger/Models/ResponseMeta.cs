using System;

namespace Quarkless.Base.ApiLogger.Models
{
	public class ResponseMeta
	{
		public string ResponseContentType { get; set; }
		public int? ResponseStatusCode { get; set; }
		//public Stream ResponseBody { get; set;  }
		public DateTime? ResponseTimestamp { get; set; }
	}
}
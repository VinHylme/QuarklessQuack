using System;
using System.Collections.Generic;

namespace QuarklessContexts.Models.APILogger
{
	public class Subject {
		public string AuthType { get; set;  }
		public string Name { get; set;  }
	}
	public class IdentityUser
	{
		public string Type { get; set; }
		public string Value { get; set;  }
		public string ValueType {get ; set;  }
	}
	public class UserDetail
	{
		public string Ip {get ; set;  }
		public IEnumerable<IdentityUser> Identity {get ;set ;}
	}

	public class RequestMeta
	{
		public string RequestContentType { get; set; }
		public string RequestUri { get; set; }
		public string RequestMethod { get; set; }
		//public Stream RequestBody { get; set;  }
		public DateTime? RequestTimestamp { get; set; }
	}

	public class ResponseMeta
	{
		public string ResponseContentType { get; set; }
		public int? ResponseStatusCode { get; set; }
		//public Stream ResponseBody { get; set;  }
		public DateTime? ResponseTimestamp { get; set; }
	}
	public class ApiLogMetaData
	{
		public RequestMeta RequestMetaData { get; set; }
		public ResponseMeta ResponseMeta { get; set;  }
		public UserDetail User { get; set;  }
		public TimeSpan TotalTimeTaken { get; set;  }
	}
}

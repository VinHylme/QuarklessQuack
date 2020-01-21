using System;

namespace Quarkless.Models.ApiLogger
{
	public class ApiLogMetaData
	{
		public RequestMeta RequestMetaData { get; set; }
		public ResponseMeta ResponseMeta { get; set; }
		public UserDetail User { get; set; }
		public TimeSpan TotalTimeTaken { get; set; }
	}
}

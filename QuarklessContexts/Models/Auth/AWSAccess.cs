using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessContexts
{
	public class AWSPool
	{
		public string AppClientID { get; set; }
		public string AppClientSecret { get; set;}
		public string AuthUrl { get; set; }
		public string PoolID { get; set; }
		public string Region { get; set; }
	}
	public class AWSAccess
	{
		public string AccessKey { get; set; }
		public string SecretKey { get; set; }
		public string Region { get; set; }

	}
}

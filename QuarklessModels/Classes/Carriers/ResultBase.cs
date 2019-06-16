using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Quarkless.Classes.Carriers
{
	public class ResultBase<T>
	{
		public string Message { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public T Results { get; set; }
	}
}

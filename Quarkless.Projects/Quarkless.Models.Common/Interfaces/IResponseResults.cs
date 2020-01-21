using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Quarkless.Models.Common.Interfaces
{
	public interface IResponseResults
	{
		int ErrorsCount { get; set; }
		string JobId { get; set; }
		HttpStatusCode StatusCode { get; set; }
		string Content { get; set; }
	}
}

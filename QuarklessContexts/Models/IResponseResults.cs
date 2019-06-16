using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace QuarklessContexts.Models
{
	public interface IResponseResults
	{
		int ErrorsCount { get; set; }
		string JobId { get; set; }
		HttpStatusCode StatusCode { get; set; }
		string Content { get; set; }

	}
}

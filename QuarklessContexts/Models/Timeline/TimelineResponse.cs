using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineResponse : IResponseResults
	{
		public int ErrorsCount { get; set ; }
		public string JobId { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string Content { get; set; }
	}
}

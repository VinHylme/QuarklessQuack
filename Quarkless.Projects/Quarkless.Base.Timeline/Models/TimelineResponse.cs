using System.Net;
using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.Timeline.Models
{
	public class TimelineResponse : IResponseResults
	{
		public int ErrorsCount { get; set ; }
		public string JobId { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string Content { get; set; }
	}
}

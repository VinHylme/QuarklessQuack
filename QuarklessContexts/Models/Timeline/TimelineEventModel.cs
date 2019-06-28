using QuarklessContexts.Models.Timeline;
using System;

namespace QuarklessContexts.Models
{
	public class TimelineEventModel
	{
		public string ActionName { get; set; }
		public RestModel Data { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

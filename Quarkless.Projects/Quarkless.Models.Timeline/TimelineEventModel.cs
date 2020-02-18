using System;

namespace Quarkless.Models.Timeline
{
	public class TimelineEventModel
	{
		public string ActionName { get; set; }
		public RestModel Data { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

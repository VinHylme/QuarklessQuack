using System;
using Quarkless.Common.Timeline.Models;

namespace Quarkless.Base.Timeline.Models
{
	public class TimelineEventModel
	{
		public string ActionName { get; set; }
		public RestModel Data { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

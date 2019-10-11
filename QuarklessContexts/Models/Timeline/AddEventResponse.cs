using System;

namespace QuarklessContexts.Models.Timeline
{
	public struct TimelineErrorResponse
	{
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
	public class AddEventResponse
	{
		public TimelineEventModel Event { get; set; }
		public bool HasCompleted { get; set; }
		public bool DailyLimitReached { get; set; }
		public bool HourlyLimitReached { get; set; }
		public bool ContainsErrors { get; set; }
		public TimelineErrorResponse Errors { get; set; }
	}
}

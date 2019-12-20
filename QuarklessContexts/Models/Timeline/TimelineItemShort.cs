using System;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineItemShort
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public string TargetId { get; set; }
		public string Body { get; set; }
	}
}
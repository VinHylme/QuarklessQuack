using System;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
		public RestModel Rest { get; set; }
	}
}

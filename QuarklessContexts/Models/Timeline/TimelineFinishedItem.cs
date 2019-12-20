using System;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineFinishedItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? SuccededAt { get; set; }
		public object Results { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
}
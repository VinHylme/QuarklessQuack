using System;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Models.Timeline
{
	public class TimelineFailedItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? FailedAt { get; set; }
		public string Error { get; set;}
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
}
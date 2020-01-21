using System;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Models.Timeline
{
	public class TimelineInProgressItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartedAt { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
}
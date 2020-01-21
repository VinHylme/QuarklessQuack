using System;
using System.Collections.Generic;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Models.Timeline
{
	public class TimelineItemDetail : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? ExpireAt { get; set; }
		public DateTimeOffset ExecuteTime { get; set; }
		public UserStoreDetails User { get; set; }	
		public string Url { get; set; }
		public RestModel Rest { get; set; }
		public List<ItemHistory> History { get; set; }
	}
}
using System;
using System.Collections.Generic;
using Quarkless.Base.Timeline.Models.Interfaces;
using Quarkless.Base.Timeline.Models.TaskScheduler;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Timeline.Models
{
	public class TimelineItemDetail : ITimelineItem
	{
		public string ItemId { get; set; }
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public UserStore User { get; set; }
		public EventActionOptions.EventBody EventBody { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? ExpireAt { get; set; }
		public DateTimeOffset ExecuteTime { get; set; }
		public List<ItemHistory> History { get; set; }
	}
}
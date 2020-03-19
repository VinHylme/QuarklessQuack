using System;
using Quarkless.Base.Timeline.Models.Interfaces;
using Quarkless.Base.Timeline.Models.TaskScheduler;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Timeline.Models
{
	public class TimelineItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public UserStore User { get; set; }
		public EventActionOptions.EventBody EventBody { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
	}
}

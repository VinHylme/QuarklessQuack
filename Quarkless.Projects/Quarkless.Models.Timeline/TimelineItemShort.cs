using System;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Models.Timeline
{
	public class TimelineItemShort
	{
		public string ItemId { get; set; }
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public EventActionOptions.EventBody Body { get; set; }
	}
}
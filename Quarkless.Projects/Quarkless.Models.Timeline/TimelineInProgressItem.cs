using System;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Models.Timeline
{
	public class TimelineInProgressItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public UserStore User { get; set; }
		public EventActionOptions.EventBody EventBody { get; set; }
		public DateTime? StartedAt { get; set; }
		public bool State { get; set; }
	}
}
using System;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Timeline.Models
{
	public class TimelineItemShort
	{
		public string ItemId { get; set; }
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public string Body { get; set; }
	}
}
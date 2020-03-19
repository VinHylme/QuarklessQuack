using Quarkless.Base.Timeline.Models.TaskScheduler;
namespace Quarkless.Base.Timeline.Models
{
	public class AddEventResponse
	{
		public EventActionOptions Event { get; set; }
		public bool HasCompleted { get; set; }
		public bool DailyLimitReached { get; set; }
		public bool HourlyLimitReached { get; set; }
		public bool ContainsErrors { get; set; }
		public TimelineErrorResponse Errors { get; set; }
	}
}

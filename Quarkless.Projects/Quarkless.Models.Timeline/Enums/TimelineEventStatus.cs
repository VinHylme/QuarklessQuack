using System.ComponentModel;

namespace Quarkless.Models.Timeline.Enums
{
	public enum TimelineEventStatus
	{
		[Description("not started")]
		NotStarted = 0,
		[Description("failed")]
		Failed = 1,
		[Description("sucess")]
		Success = 2,
		[Description("feedback")]
		FeedbackRequired = 3,
		[Description("Internal Server Error")]
		ServerError
	}
}

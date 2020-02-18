using System.ComponentModel;

namespace Quarkless.Models.Notification.Enums
{
	public enum TimelineEventItemStatus
	{
		[Description("not started")]
		NotStarted = 0,
		[Description("sucess")]
		Success = 1,
		[Description("failed")]
		Failed = 2,
		[Description("Internal Server Error")]
		ServerError = 3,
		[Description("feedback")]
		FeedbackRequired = 4,
	}
}
using System.ComponentModel;

namespace Quarkless.Models.Timeline.Enums
{
	public enum ActionExecuteStatus
	{
		[Description("failed")]
		Failed = 1,
		[Description("sucess")]
		Success = 2,
	}
}
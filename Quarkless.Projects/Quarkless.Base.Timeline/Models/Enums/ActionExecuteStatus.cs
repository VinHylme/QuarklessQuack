using System.ComponentModel;

namespace Quarkless.Base.Timeline.Models.Enums
{
	public enum ActionExecuteStatus
	{
		[Description("failed")]
		Failed = 1,
		[Description("sucess")]
		Success = 2,
	}
}
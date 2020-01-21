using System;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.ActionOptions
{
	public class LikeActionOptions : IActionOptions
	{
		public LikeActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public LikeActionOptions(DateTimeOffset executionTime, LikeActionType likeActionType)
		{
			this.ExecutionTime = executionTime;
			this.LikeActionType = likeActionType;
		}
	}
}
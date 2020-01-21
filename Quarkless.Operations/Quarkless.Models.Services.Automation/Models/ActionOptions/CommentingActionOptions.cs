using System;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.ActionOptions
{
	public class CommentingActionOptions : IActionOptions
	{
		public CommentingActionType CommentingActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);

		public CommentingActionOptions(DateTimeOffset executionTime, CommentingActionType commentingActionType)
		{
			this.ExecutionTime = executionTime;
			this.CommentingActionType = commentingActionType;
		}
	}
}
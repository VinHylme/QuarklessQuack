using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
{
	public class CommentingActionOptions : IActionOptions
	{
		public CommentingActionType CommentingActionType { get; set; } = CommentingActionType.Any;
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CommentingActionDailyLimit { get; set; } = new Range(400, 500);
		public static Range CommentingActionHourlyLimit { get; set; } = new Range(30, 60);
		public static Range TimeFrameSeconds { get; set; } = new Range(4, 8);

		public CommentingActionOptions(DateTimeOffset executionTime, CommentingActionType commentingActionType)
		{
			this.ExecutionTime = executionTime;
			this.CommentingActionType = commentingActionType;
		}
	}
}

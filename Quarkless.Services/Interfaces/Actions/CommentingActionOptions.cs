using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
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

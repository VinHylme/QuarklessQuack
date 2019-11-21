using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
{
	public class LikeCommentActionOptions : IActionOptions
	{
		public LikeCommentActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public LikeCommentActionOptions(DateTimeOffset executionTime, LikeCommentActionType likeActionType)
		{
			this.ExecutionTime = executionTime;
			this.LikeActionType = likeActionType;
		}
	}

}

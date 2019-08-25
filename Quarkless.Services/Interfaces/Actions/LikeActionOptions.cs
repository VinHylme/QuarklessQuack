using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
{
	public class LikeActionOptions : IActionOptions
	{
		public LikeActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range TimeFrameSeconds { get; set; } = new Range(45, 75);
		public LikeActionOptions(DateTimeOffset executionTime, LikeActionType likeActionType)
		{
			this.ExecutionTime = executionTime;
			this.LikeActionType = likeActionType;
		}
	}
}

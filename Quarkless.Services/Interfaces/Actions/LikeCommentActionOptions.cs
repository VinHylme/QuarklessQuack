using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class LikeCommentActionOptions : IActionOptions
	{
		public LikeCommentActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range LikeActionDailyLimit { get; set; } = new Range(900, 950);
		public static Range LikeActionHourlyLimit { get; set; } = new Range(30, 55);
		public static Range TimeFrameSeconds { get; set; } = new Range(15, 33);
		public LikeCommentActionOptions(DateTimeOffset executionTime, LikeCommentActionType likeActionType)
		{
			this.ExecutionTime = executionTime;
			this.LikeActionType = likeActionType;
		}
	}

}

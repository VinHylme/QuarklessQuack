using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class LikeActionOptions : IActionOptions
	{
		public LikeActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range LikeActionDailyLimit { get; set; } = new Range(900,1000);
		public static Range LikeActionHourlyLimit { get; set; } = new Range(30,60);
		public static Range TimeFrameSeconds { get; set; } = new Range(10,20);
		public LikeActionOptions(DateTimeOffset executionTime, LikeActionType likeActionType)
		{
			this.ExecutionTime = executionTime;
			this.LikeActionType = likeActionType;
		}
	}
}

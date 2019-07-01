using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class FollowActionOptions : IActionOptions
	{
		public FollowActionType FollowActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range FollowActionDailyLimit { get; set; } = new Range(200, 250);
		public static Range FollowActionHourlyLimit { get; set; } = new Range(30, 60);
		public static Range TimeFrameSeconds { get; set; } = new Range(33, 66);

		public FollowActionOptions(DateTimeOffset executionTime, FollowActionType followActionType)
		{
			this.ExecutionTime = executionTime;
			this.FollowActionType = followActionType;
		}
	}
}

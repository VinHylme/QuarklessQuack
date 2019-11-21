using System;
using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class FollowActionOptions : IActionOptions
	{
		public FollowActionType FollowActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);

		public FollowActionOptions(DateTimeOffset executionTime, FollowActionType followActionType)
		{
			this.ExecutionTime = executionTime;
			this.FollowActionType = followActionType;
		}
	}
}

using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class UnfollowActionOptions : IActionOptions
	{
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range UnFollowActionDailyLimit { get; set; } = new Range(200, 225);
		public static Range UnFollowActionHourlyLimit { get; set; } = new Range(30, 60);
		public static Range TimeFrameSeconds { get; set; } = new Range(10, 22);

		public UnfollowActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

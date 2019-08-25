using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
{
	public class UnfollowActionOptions : IActionOptions
	{
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range TimeFrameSeconds { get; set; } = new Range(45, 75);

		public UnfollowActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

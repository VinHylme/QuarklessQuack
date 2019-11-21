using QuarklessContexts.Models;
using System;

namespace Quarkless.Services.Interfaces.Actions
{
	public class UnfollowActionOptions : IActionOptions
	{
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);

		public UnfollowActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

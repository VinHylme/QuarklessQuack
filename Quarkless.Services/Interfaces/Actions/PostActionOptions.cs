using System;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class PostActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CreatePostActionDailyLimit { get; set; } = new Range(10, 30);
		public static Range CreatePostActionHourlyLimit { get; set; } = new Range(1, 4);
		public static Range TimeFrameSeconds { get; set; } = new Range(2500, 3250);

		public PostActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class VideoActionOptions : IActionOptions
	{
		public int VideoFetchLimit { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CreateVideoActionDailyLimit { get; set; } = new Range(10, 15);
		public static Range CreateVideoActionHourlyLimit { get; set; } = new Range(1, 2);
		public static Range TimeFrameSeconds { get; set; } = new Range(120,420);
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class ImageActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CreatePhotoActionDailyLimit { get; set; } = new Range(10, 15);
		public static Range CreatePhotoActionHourlyLimit { get; set; } = new Range(1, 2);
		public static Range TimeFrameSeconds { get; set; } = new Range(90, 360);

		public ImageActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

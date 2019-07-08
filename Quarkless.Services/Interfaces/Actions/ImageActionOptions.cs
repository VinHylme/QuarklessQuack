﻿using System;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class ImageActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CreatePhotoActionDailyLimit { get; set; } = new Range(10, 25);
		public static Range CreatePhotoActionHourlyLimit { get; set; } = new Range(1, 3);
		public static Range TimeFrameSeconds { get; set; } = new Range(60, 140);

		public ImageActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

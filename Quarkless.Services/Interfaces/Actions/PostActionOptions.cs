using System;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class PostActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(1500, 2000);

		public PostActionOptions(DateTimeOffset executionTime)
		{
			this.ExecutionTime = executionTime;
		}
	}
}

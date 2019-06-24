using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.StrategyBuilders;

namespace Quarkless.Services.Interfaces.Actions
{
	public class VideoActionOptions : IActionOptions
	{
		public int VideoFetchLimit { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

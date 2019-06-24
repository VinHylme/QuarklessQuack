using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.StrategyBuilders;

namespace Quarkless.Services.Interfaces.Actions
{
	public class ImageActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

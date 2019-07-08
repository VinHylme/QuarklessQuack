using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class AccountCheckerActionOptions : IActionOptions
	{
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range TimeFrameSeconds { get; set; } = new Range(1800, 2000);

	}
}

using Quarkless.Analyser;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Interfaces;
using System;

namespace Quarkless.Models.Services.Automation.Models.ActionOptions
{
	public class AccountCheckerActionOptions : IActionOptions
	{
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(1800, 2000);
		public IPostAnalyser PostAnalyser { get; }
		public AccountCheckerActionOptions(IPostAnalyser postAnalyser)
		{
			PostAnalyser = postAnalyser;
		}
	}
}

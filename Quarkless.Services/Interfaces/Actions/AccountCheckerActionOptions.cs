using QuarklessContexts.Models;
using System;
using Quarkless.Analyser;

namespace Quarkless.Services.Interfaces.Actions
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

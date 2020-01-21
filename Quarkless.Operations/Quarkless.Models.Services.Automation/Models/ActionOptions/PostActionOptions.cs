using System;
using Quarkless.Analyser;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.ActionOptions
{
	public class PostActionOptions : IActionOptions
	{
		public int ImageFetchLimit { get; set; } = 20;
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(1500, 2000);
		public IPostAnalyser PostAnalyser { get; }
		public PostActionOptions(DateTimeOffset executionTime, IPostAnalyser postAnalyser)
		{
			this.ExecutionTime = executionTime;
			this.PostAnalyser = postAnalyser;
		}
	}
}
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces
{
	public interface IActionOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
	public interface IActionCommit
	{
		IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions);
		IActionCommit IncludeStrategy(IStrategySettings strategy);
	}
}

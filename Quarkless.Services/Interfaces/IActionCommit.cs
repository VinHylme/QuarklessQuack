using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;

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
		IActionCommit IncludeUser(UserStoreDetails userStoreDetails);
	}
}

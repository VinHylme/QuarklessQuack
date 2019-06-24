using Quarkless.Services.StrategyBuilders;
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
		void Push(IActionOptions actionOptions);
		IActionCommit IncludeStrategy(IStrategy strategy);
	}
}

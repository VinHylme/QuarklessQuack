using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.StrategyBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class LikeActionOptions : IActionOptions
	{
		public LikeActionType LikeActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

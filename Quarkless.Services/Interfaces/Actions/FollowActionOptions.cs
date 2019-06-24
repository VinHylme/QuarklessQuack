using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.StrategyBuilders;

namespace Quarkless.Services.Interfaces.Actions
{
	public class FollowActionOptions : IActionOptions
	{
		public FollowActionType FollowActionType { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

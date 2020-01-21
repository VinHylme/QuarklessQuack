using System;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class AccountCheckerStrategySettings : IStrategySettings
	{
		public AccountCheckerStrategy AccountCheckerStrategy { get; set ;}
		public TimeSpan OffsetPerAction { get; set; }
	}
}

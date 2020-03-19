using System;
using Quarkless.Base.Actions.Models.Enums.StrategyType;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings
{
	public class AccountCheckerStrategySettings
	{
		public AccountCheckerStrategy StrategyType { get; set; }
		public TimeSpan OffsetPerAction { get; set; } = TimeSpan.FromSeconds(2);

		public AccountCheckerStrategySettings(
			AccountCheckerStrategy strategyType = AccountCheckerStrategy.Default, 
			TimeSpan? offsetPerAction = null)
		{
			StrategyType = strategyType;
			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
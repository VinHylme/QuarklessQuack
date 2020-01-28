using System;
using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class FollowStrategySettings
	{
		public FollowStrategyType StrategyType { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; } = TimeSpan.FromSeconds(2);

		public FollowStrategySettings(FollowStrategyType strategyType = FollowStrategyType.Default,
			TimeSpan? offsetPerAction = null, int numberOfActions = 1)
		{
			this.StrategyType = strategyType;
			this.NumberOfActions = numberOfActions;

			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
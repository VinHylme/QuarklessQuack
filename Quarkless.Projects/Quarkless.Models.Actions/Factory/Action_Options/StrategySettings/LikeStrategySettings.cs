using System;
using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class LikeStrategySettings
	{
		public LikeStrategyType StrategyType { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }

		public LikeStrategySettings(LikeStrategyType strategyType = LikeStrategyType.Default,
			TimeSpan? offsetPerAction = null, int numberOfActions = 1)
		{
			this.StrategyType = strategyType;
			this.NumberOfActions = numberOfActions;
			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
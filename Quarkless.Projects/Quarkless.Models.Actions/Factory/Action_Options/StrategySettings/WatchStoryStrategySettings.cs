using System;
using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class WatchStoryStrategySettings
	{
		public WatchStoryStrategyType StrategyType { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }

		public WatchStoryStrategySettings(WatchStoryStrategyType strategyType = WatchStoryStrategyType.Default,
			TimeSpan? offsetPerAction = null, int numberOfActions = 1)
		{
			StrategyType = strategyType;
			NumberOfActions = numberOfActions;
			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
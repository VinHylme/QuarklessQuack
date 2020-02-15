using System;
using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class ReactStoryStrategySettings
	{
		public ReactStoryStrategyType StrategyType { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }

		public ReactStoryStrategySettings(ReactStoryStrategyType strategyType = ReactStoryStrategyType.Default,
			TimeSpan? offsetPerAction = null, int numberOfActions = 1)
		{
			StrategyType = strategyType;
			NumberOfActions = numberOfActions;
			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
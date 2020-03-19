using System;
using Quarkless.Base.Actions.Models.Enums.StrategyType;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings
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
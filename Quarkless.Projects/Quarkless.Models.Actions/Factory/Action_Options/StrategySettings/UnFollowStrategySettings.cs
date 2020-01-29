using System;
using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class UnFollowStrategySettings
	{
		public UnFollowStrategyType StrategyType { get; set; }
		public int NumberOfUnfollows { get; set; }
		public TimeSpan OffsetPerAction { get; set; }

		public UnFollowStrategySettings(UnFollowStrategyType strategyType = UnFollowStrategyType.Default,
			TimeSpan? offsetPerAction = null, int numberOfUnfollows = 1)
		{
			this.StrategyType = strategyType;
			this.NumberOfUnfollows = numberOfUnfollows;

			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
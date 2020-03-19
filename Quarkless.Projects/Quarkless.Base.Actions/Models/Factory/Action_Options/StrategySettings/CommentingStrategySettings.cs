using System;
using Quarkless.Base.Actions.Models.Enums.StrategyType;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings
{
	public class CommentingStrategySettings
	{
		public CommentingStrategy StrategyType { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; } = TimeSpan.FromSeconds(2);
		public CommentingStrategySettings(CommentingStrategy strategyType = CommentingStrategy.Default,
			TimeSpan? offsetPerAction = null, int numberOfActions = 1)
		{
			this.StrategyType = strategyType;
			this.NumberOfActions = numberOfActions;

			if (offsetPerAction.HasValue)
				OffsetPerAction = offsetPerAction.Value;
		}
	}
}
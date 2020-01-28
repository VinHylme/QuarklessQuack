using Quarkless.Models.Actions.Enums.StrategyType;

namespace Quarkless.Models.Actions.Factory.Action_Options.StrategySettings
{
	public class PostStrategySettings
	{
		public PostStrategyType StrategyType { get; set; }

		public PostStrategySettings(PostStrategyType strategy = PostStrategyType.Default)
		{
			this.StrategyType = strategy;
		}
	}
}
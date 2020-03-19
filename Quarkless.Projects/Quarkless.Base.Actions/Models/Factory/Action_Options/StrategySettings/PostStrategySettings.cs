using Quarkless.Base.Actions.Models.Enums.StrategyType;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings
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
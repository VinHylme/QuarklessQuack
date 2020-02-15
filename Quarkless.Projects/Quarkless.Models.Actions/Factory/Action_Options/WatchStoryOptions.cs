using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class WatchStoryOptions : IActionOptions
	{
		public StoryActionType storyActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(25, 45);
		public WatchStoryStrategySettings StrategySettings { get; set; } = new WatchStoryStrategySettings();

		public WatchStoryOptions(StoryActionType storyActionType = StoryActionType.Any,
			WatchStoryStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.storyActionType = storyActionType;
			if (strategySettings != null)
				StrategySettings = strategySettings;
			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}
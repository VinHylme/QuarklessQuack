using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options
{
	public class ReactStoryOptions : IActionOptions
	{
		public StoryActionType storyActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(25, 45);
		public ReactStoryStrategySettings StrategySettings { get; set; } = new ReactStoryStrategySettings();

		public ReactStoryOptions(StoryActionType storyActionType = StoryActionType.Any,
			ReactStoryStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.storyActionType = storyActionType;
			if (strategySettings != null)
				StrategySettings = strategySettings;
			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}
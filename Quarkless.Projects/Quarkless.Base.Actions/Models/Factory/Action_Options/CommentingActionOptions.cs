using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options
{
	public class CommentingActionOptions : IActionOptions
	{
		public CommentingActionType CommentingActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public CommentingStrategySettings StrategySettings { get; set; } = new CommentingStrategySettings();

		public CommentingActionOptions(CommentingActionType commentingActionType = CommentingActionType.Any,
			CommentingStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.CommentingActionType = commentingActionType;

			if (strategySettings != null)
				StrategySettings = strategySettings;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}
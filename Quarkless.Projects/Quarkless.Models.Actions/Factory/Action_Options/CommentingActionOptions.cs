using System;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
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
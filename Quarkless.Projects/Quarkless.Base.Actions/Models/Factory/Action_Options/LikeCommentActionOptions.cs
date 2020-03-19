using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options
{
	public class LikeCommentActionOptions : IActionOptions
	{
		public LikeCommentActionType LikeActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public LikeStrategySettings StrategySettings { get; set; } = new LikeStrategySettings();

		public LikeCommentActionOptions(LikeCommentActionType likeActionType = LikeCommentActionType.Any,
			LikeStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.LikeActionType = likeActionType;

			if (strategySettings != null)
				StrategySettings = strategySettings;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}
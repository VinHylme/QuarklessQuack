using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class FollowActionOptions : IActionOptions
	{
		public FollowActionType FollowActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public FollowStrategySettings StrategySettings { get; set; } = new FollowStrategySettings();
		
		public FollowActionOptions(FollowActionType followActionType = FollowActionType.Any,
			FollowStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.FollowActionType = followActionType;
			
			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;

			if (strategySettings != null)
				StrategySettings = strategySettings;

		}
	}
}
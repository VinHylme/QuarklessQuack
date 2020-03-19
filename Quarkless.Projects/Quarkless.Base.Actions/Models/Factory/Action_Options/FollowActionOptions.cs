using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options
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
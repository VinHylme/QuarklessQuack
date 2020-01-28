using System;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class UnfollowActionOptions : IActionOptions
	{
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public UnFollowStrategySettings StrategySettings { get; set; } = new UnFollowStrategySettings();
		public UnfollowActionOptions(UnFollowStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			if (strategySettings != null)
				StrategySettings = strategySettings;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}
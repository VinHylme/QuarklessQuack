using Quarkless.Analyser;
using Quarkless.Base.Actions.Models.Factory.Action_Options.StrategySettings;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Actions.Models.Factory.Action_Options
{
	public class AccountCheckerActionOptions : IActionOptions
	{
		public static XRange TimeFrameSeconds { get; set; } = new XRange(1800, 2000);
		public AccountCheckerStrategySettings StrategySettings = new AccountCheckerStrategySettings();
		public readonly IPostAnalyser PostAnalyser;

		public AccountCheckerActionOptions(IPostAnalyser postAnalyser, AccountCheckerStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			PostAnalyser = postAnalyser;
			if (strategySettings != null)
				StrategySettings = strategySettings;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}

using Quarkless.Analyser;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
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

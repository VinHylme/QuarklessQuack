using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class VideoStrategySettings : IStrategySettings
	{
		public VideoStrategyType VideoStrategyType { get; set; } = VideoStrategyType.Default;
	}
}
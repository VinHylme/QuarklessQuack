using Quarkless.Models.Services.Automation.Enums.Actions;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class ImageStrategySettings : IStrategySettings
	{
		public ImageStrategyType ImageStrategyType { get; set; } = ImageStrategyType.Default;
	}
}
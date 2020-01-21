using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class CarouselStrategySettings : IStrategySettings
	{
		public CarouselStrategyType CarouselStrategy { get; set; } = CarouselStrategyType.Default;
	}
}
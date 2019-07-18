using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum CarouselStrategyType
	{
		Default
	}
	public class CarouselStrategySettings : IStrategySettings
	{
		public CarouselStrategyType CarouselStrategy { get; set; } = CarouselStrategyType.Default;
	}
}

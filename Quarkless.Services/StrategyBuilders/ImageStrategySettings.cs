using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum ImageStrategyType
	{
		Default
	}
	public class ImageStrategySettings : IStrategy
	{
		public ImageStrategyType ImageStrategyType { get; set; } = ImageStrategyType.Default;
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum LikeStrategyType
	{
		Default,
		TwoDollarCent
	}
	public class LikeStrategySettings : IStrategy
	{
		public LikeStrategyType LikeStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public DateTimeOffset OffsetPerAction { get; set; }
	}
}

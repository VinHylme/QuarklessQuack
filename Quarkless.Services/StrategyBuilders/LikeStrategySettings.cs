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
	public class LikeStrategySettings : IStrategySettings
	{
		public LikeStrategyType LikeStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }
	}
}

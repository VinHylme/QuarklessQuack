using System;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class LikeStrategySettings : IStrategySettings
	{
		public LikeStrategyType LikeStrategy { get; set; } = LikeStrategyType.Default;
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }
	}
}
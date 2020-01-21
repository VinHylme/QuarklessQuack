using System;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class UnFollowStrategySettings : IStrategySettings
	{
		public UnFollowStrategyType UnFollowStrategy { get; set; }
		public int NumberOfUnfollows { get; set; }
		public TimeSpan OffsetPerAction { get; set; }
	}
}
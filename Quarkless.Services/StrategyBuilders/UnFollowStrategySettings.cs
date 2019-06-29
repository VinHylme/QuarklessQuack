using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum UnFollowStrategyType
	{
		Default,
		LeastEngagingN
	}
	public class UnFollowStrategySettings : IStrategySettings
	{
		public UnFollowStrategyType UnFollowStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public DateTimeOffset OffsetPerAction { get; set; }
	}
}

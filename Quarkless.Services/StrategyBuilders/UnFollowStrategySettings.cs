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
		public int NumberOfUnfollows { get; set; }
		public TimeSpan OffsetPerAction { get; set; }
	}
}

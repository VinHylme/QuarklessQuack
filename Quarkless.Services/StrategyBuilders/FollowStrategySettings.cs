using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum FollowStrategyType
	{
		Default
	}
	public class FollowStrategySettings : IStrategy
	{
		public FollowStrategyType FollowStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public DateTimeOffset OffsetPerAction { get; set; }
	}
}

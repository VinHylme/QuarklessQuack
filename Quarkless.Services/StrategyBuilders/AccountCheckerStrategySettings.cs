using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum AccountCheckerStrategy
	{
		Default
	}
	public class AccountCheckerStrategySettings : IStrategySettings
	{
		public AccountCheckerStrategy AccountCheckerStrategy { get; set ;}
		public TimeSpan OffsetPerAction { get; set; }
	}
}

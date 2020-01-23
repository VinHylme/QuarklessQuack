﻿using System;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class FollowStrategySettings : IStrategySettings
	{
		public FollowStrategyType FollowStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public DateTimeOffset OffsetPerAction { get; set; }
	}
}
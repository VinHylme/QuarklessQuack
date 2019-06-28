using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum VideoStrategyType
	{
		Default,

	}
	public class VideoStrategySettings : IStrategySettings
	{
		public VideoStrategyType VideoStrategyType { get; set; } = VideoStrategyType.Default;
	}
}

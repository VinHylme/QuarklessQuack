using System;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.StrategySettings
{
	public class CommentingStrategySettings : IStrategySettings
	{
		public CommentingStrategy CommentingStrategy { get; set; }
		public int NumberOfActions { get; set; }
		public TimeSpan OffsetPerAction { get; set; }
		public CommentingStrategySettings(CommentingStrategy commentingStrategy = CommentingStrategy.Default)
		{
			this.CommentingStrategy = commentingStrategy;
		}
	}
}
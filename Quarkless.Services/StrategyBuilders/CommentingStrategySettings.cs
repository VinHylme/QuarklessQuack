using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum CommentingStrategy
	{
		Default,
		TopNth
	}
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

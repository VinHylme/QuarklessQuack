using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.StrategyBuilders
{
	public enum CommentingStrategy
	{
		Default
	}
	public class CommentingStrategySettings : IStrategy
	{
		public CommentingStrategy CommentingStrategy { get; set; }
	}
}

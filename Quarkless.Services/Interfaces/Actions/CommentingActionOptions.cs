using Quarkless.Services.ActionBuilders.EngageActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class CommentingActionOptions : IActionOptions
	{
		public CommentingActionType CommentingActionType { get; set; } = CommentingActionType.Any;
		public DateTimeOffset ExecutionTime { get; set; }
	}
}

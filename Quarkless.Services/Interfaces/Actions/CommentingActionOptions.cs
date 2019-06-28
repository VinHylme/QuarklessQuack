using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces.Actions
{
	public class CommentingActionOptions : IActionOptions
	{
		public CommentingActionType CommentingActionType { get; set; } = CommentingActionType.Any;
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range CommentingActionDailyLimit { get; set; } = new Range(400, 500);
		public static Range CommentingActionHourlyLimit { get; set; } = new Range(30, 60);
	}
}

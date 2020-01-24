using System;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Logic.Services.Automation.Actions.ActionManager
{
	public class CommitContainer
	{
		public ActionType ActionType;
		public IActionCommit Action;
		public IActionOptions Options;
		public DateTime? Remaining { get; set; } = null;
		public CommitContainer(IActionCommit actionCommit, IActionOptions actionOptions)
		{
			Action = actionCommit;
			Options = actionOptions;
			ActionType = Action.GetActionType().Value;
		}
	}
}
using System;
using Quarkless.Logic.Actions.AccountContainer.Extensions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Logic.Actions.AccountContainer
{
	public class CommitContainer
	{
		public ActionType ActionType;
		public IActionCommit Action;
		public DateTimeOffset ExecTime;
		public DateTime? Remaining { get; set; } = null;

		public CommitContainer(IActionCommit actionCommit, DateTimeOffset execTime)
		{
			Action = actionCommit;
			ExecTime = execTime;
			ActionType = Action.GetActionType();
		}
	}
}
using System;
using Quarkless.Base.Actions.Logic.AccountContainer.Extensions;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Actions.Logic.AccountContainer
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
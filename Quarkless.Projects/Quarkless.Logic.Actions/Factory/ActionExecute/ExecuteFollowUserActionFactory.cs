﻿using Quarkless.Logic.Actions.Action_Executes;
using Quarkless.Models.Actions.Factory;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Factory.ActionExecute
{
	public class ExecuteFollowUserActionFactory : ActionExecuteFactory
	{
		public override IActionExecute Create(IWorker worker, IResponseResolver resolver)
			=> new FollowUserExecute(worker, resolver);
	}
}
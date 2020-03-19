using Quarkless.Base.Actions.Logic.Action_Executes;
using Quarkless.Base.Actions.Models.Factory;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.Actions.Logic.Factory.ActionExecute
{
	public class ExecuteCommentMediaActionFactory : ActionExecuteFactory
	{
		public override IActionExecute Create(IWorker worker, IResponseResolver resolver)
			=> new CreateCommentExecute(worker, resolver);
	}
}

using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.Actions.Factory
{
	public abstract class ActionExecuteFactory
	{
		public abstract IActionExecute Create(IWorker worker, IResponseResolver resolver);
	}
}
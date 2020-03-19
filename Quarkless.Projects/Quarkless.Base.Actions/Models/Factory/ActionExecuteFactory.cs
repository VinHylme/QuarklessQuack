using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.Actions.Models.Factory
{
	public abstract class ActionExecuteFactory
	{
		public abstract IActionExecute Create(IWorker worker, IResponseResolver resolver);
	}
}
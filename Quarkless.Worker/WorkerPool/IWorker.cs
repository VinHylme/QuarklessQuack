using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.InstagramAccounts;

namespace Quarkless.Worker.WorkerPool
{
	public interface IWorker
	{
		Task<bool> Begin(SchedulerSettings schedulerSettings);
		Task<IEnumerable<InstagramAccountModel>> GetAccount(params string[] accounts);
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Timeline.Interfaces
{
	public interface IActionExecuteLogsRepository
	{
		Task AddActionLog(ActionExecuteLog log);
		Task<IEnumerable<ActionExecuteLog>> GetActionLogs(string accountId, string instagramAccountId, int limit = 250);
	}
}
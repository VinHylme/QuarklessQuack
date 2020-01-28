using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Models.Actions.Interfaces
{
	public interface IActionExecute
	{
		Task<ResultCarrier<bool>> ExecuteAsync(EventExecuteBody eventAction);
	}
}
using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Actions.Models.Interfaces
{
	public interface IActionExecute
	{
		Task<ResultCarrier<bool>> ExecuteAsync(EventExecuteBody eventAction);
	}
}
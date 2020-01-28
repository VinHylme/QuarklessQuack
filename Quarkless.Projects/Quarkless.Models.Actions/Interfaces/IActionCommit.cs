using System;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Models.Actions.Interfaces
{
	public interface IActionCommit
	{
		Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime);
		IActionCommit ModifyOptions(IActionOptions newOptions);
	}
}

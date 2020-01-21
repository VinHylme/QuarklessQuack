using System.Collections.Generic;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Models.Services.Automation.Interfaces
{
	public interface IActionCommit
	{
		ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions);
		IActionCommit IncludeStrategy(IStrategySettings strategy);
		IActionCommit IncludeUser(UserStoreDetails userStoreDetails);
		IActionCommit IncludeStorage(IStorage storage);
	}
}

using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Actions.Models.Interfaces
{
	public interface IActionCommitFactory
	{
		public IActionCommit Create(ActionType action, in UserStoreDetails user);
	}
}
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Actions.Interfaces
{
	public interface IActionCommitFactory
	{
		public IActionCommit Create(ActionType action, in UserStoreDetails user);
	}
}
using Quarkless.Models.Actions.Enums;
using Quarkless.Models.Timeline;

namespace Quarkless.Models.Actions.Interfaces
{
	public interface IActionCommitFactory
	{
		public IActionCommit Create(ActionType action, in UserStoreDetails user);
	}
}
using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Services.Automation.Interfaces
{
	public interface IActionFactory
	{
		public IActionCommit Commit(ActionType action);
	}
}

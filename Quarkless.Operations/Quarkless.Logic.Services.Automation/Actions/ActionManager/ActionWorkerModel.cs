using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Logic.Services.Automation.Actions.ActionManager
{
	public class ActionWorkerModel
	{
		public CommitContainer ActionContainer { get; set; }
		public ActionState ActionState { get; set; }
		public ErrorResponse Response { get; set; }
	}
}
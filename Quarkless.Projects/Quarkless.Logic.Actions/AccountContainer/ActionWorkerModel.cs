using Quarkless.Models.Actions.Enums;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Logic.Actions.AccountContainer
{
	public class ActionWorkerModel
	{
		public CommitContainer ActionContainer { get; set; }
		public ActionState ActionState { get; set; }
		public ErrorResponse Response { get; set; }
	}
}
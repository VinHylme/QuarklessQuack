using Quarkless.Base.Actions.Models.Enums;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Actions.Logic.AccountContainer
{
	public class ActionWorkerModel
	{
		public CommitContainer ActionContainer { get; set; }
		public ActionState ActionState { get; set; }
		public ErrorResponse Response { get; set; }
	}
}
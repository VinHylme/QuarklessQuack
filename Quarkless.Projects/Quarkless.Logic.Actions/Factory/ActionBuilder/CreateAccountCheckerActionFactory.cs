using Quarkless.Logic.Actions.Action_Instances;
using Quarkless.Models.Actions.Factory;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Actions.Factory.ActionBuilder
{
	public class ExecuteAccountCheckerActionFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStoreDetails user, IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic)
			=> new AccountCheckerAction(user, contentInfoBuilder, heartbeatLogic);
	}
}
using Quarkless.Logic.Actions.Action_Instances;
using Quarkless.Models.Actions.Factory;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;

namespace Quarkless.Logic.Actions.Factory.ActionBuilder
{
	public class ExecuteUnfollowUserActionFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStoreDetails user, IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic)
			=> new UnFollowUserAction(user, contentInfoBuilder, heartbeatLogic);
	}
}
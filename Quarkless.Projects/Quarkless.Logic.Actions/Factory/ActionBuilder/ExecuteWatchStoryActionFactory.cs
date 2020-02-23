using Quarkless.Logic.Actions.Action_Instances;
using Quarkless.Models.Actions.Factory;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Lookup.Interfaces;

namespace Quarkless.Logic.Actions.Factory.ActionBuilder
{
	public class ExecuteWatchStoryActionFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStoreDetails userStoreDetails, IContentInfoBuilder builder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			=> new WatchStoryAction(userStoreDetails, builder, heartbeatLogic, lookupLogic);
	}
}
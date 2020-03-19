using Quarkless.Base.Actions.Logic.Action_Instances;
using Quarkless.Base.Actions.Models.Factory;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Common.Timeline.Models;

namespace Quarkless.Base.Actions.Logic.Factory.ActionBuilder
{
	public class ExecuteCommentMediaActionFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			=> new CreateCommentAction(userStoreDetails, contentInfoBuilder, heartbeatLogic, lookupLogic);
	}
}

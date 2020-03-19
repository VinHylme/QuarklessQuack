using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Common.Timeline.Models;

namespace Quarkless.Base.Actions.Models.Factory
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(UserStoreDetails userStoreDetails,
			IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic);
	}
}

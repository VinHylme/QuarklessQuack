using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;

namespace Quarkless.Models.Actions.Factory
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(UserStoreDetails userStoreDetails, IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic);
	}
}

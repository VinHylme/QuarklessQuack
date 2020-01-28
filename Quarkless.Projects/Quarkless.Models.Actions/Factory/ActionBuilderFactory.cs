using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Models.Actions.Factory
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(UserStoreDetails userStoreDetails, IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic);
	}
}

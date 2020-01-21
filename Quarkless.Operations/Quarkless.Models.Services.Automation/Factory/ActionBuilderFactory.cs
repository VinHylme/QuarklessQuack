using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Factory
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader);
	}
}

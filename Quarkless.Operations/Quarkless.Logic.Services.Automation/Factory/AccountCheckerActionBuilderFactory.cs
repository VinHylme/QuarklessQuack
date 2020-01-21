using Quarkless.Logic.Services.Automation.Actions.MaintainActions;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.Services.Automation.Factory;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Logic.Services.Automation.Factory
{
	public class AccountCheckerActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
			=> new AccountCheckerAction(builder,heartbeatLogic, urlReader);
	}
}

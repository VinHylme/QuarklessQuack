using Quarkless.Services.ActionBuilders.MaintainActions;
using Quarkless.Services.Interfaces;
using QuarklessLogic.Handlers.ContentInfoBuilder;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	public class AccountCheckerActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
			=> new AccountCheckerAction(builder,heartbeatLogic, urlReader);
	}
}

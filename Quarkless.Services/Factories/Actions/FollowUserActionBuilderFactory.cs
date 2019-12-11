using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	public class FollowUserActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
			=> new FollowUserAction(builder,heartbeatLogic, urlReader);
	}
}

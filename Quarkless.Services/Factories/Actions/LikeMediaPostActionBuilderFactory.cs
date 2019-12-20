using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessLogic.Handlers.ContentInfoBuilder;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	class LikeMediaPostActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
			   => new LikeMediaAction(builder,heartbeatLogic, urlReader);
	}

}

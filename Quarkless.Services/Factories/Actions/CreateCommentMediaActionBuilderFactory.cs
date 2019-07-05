using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	public class CreateCommentMediaActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, IHeartbeatLogic heartbeatLogic, ProfileModel profile)
			=> new CreateCommentAction(builder,heartbeatLogic,profile);
	}
}

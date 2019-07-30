using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(IContentManager builder, IHeartbeatLogic heartbeatLogic);
	}
}

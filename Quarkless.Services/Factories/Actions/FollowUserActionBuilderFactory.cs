using System;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.Factories.Actions
{
	public class FollowUserActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, ProfileModel profile, DateTime executeTime)
			=> new FollowUserAction(builder,profile,executeTime);
	}
}

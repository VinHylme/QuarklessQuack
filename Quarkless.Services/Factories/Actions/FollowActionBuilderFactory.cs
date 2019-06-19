using System;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.Factories.Actions
{
	public class FollowActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
			=> new FollowAction(userSession,builder,profile,executeTime);
	}
}

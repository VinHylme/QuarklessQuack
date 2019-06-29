using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.Factories.Actions
{
	public class UnFollowUserActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, ProfileModel profile, UserStore user) => new UnFollowUserAction(builder,profile,user);
	}
}

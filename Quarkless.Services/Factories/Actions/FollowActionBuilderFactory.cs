using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.Factories.Actions
{
	public class FollowActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStore userSession, IActionBuilderManager builder, ProfileModel profile, DateTime executeTime)
			=> new FollowAction(userSession,builder,profile,executeTime);
	}
}

using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories.Actions
{
	class LikeMediaPostActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
			   => new LikeMediaAction(userSession, builder, profile, executeTime);
	}

}

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
		public override IActionCommit Commit(IContentManager builder, ProfileModel profile)
			   => new LikeMediaAction(builder, profile);
	}

}

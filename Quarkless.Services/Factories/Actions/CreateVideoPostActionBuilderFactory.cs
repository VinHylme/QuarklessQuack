using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;

namespace Quarkless.Services.Factories.Actions
{
	class CreateVideoPostActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, ProfileModel profile, DateTime executeTime)
			   => new CreateVideoPost(builder, profile, executeTime);
	}
}

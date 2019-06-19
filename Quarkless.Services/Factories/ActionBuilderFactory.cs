using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;

namespace Quarkless.Services.Factories
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime);

	}
}

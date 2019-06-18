using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public abstract class ActionBuilderFactory
	{
		public abstract IActionCommit Commit(UserStore userSession, IActionBuilderManager builder, ProfileModel profile, DateTime executeTime);

	}
}

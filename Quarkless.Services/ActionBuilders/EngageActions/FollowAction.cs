using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class FollowAction : IActionCommit
	{
		private readonly UserStore _userStore;
		private readonly IActionBuilderManager _actionBuilder;
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		public FollowAction(UserStore userSession, IActionBuilderManager builder, ProfileModel profile, DateTime executeTime)
		{
			_userStore = userSession;
			_actionBuilder = builder;
			_profile = profile;
			_executeTime = executeTime;
		}

		public void Operate()
		{
			
		}
	}
}

using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class FollowAction : IActionCommit
	{
		private readonly UserStore _userStore;
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		public FollowAction(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
		{
			_userStore = userSession;
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
		}

		public void Operate()
		{
			
		}
	}
}

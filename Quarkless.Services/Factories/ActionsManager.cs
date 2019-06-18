using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public class ActionsManager
	{
		private readonly Dictionary<ActionType, ActionBuilderFactory> _factories;

		public ActionsManager()
		{
			_factories = new Dictionary<ActionType, ActionBuilderFactory>
			{
				{ ActionType.Follow, new FollowActionBuilderFactory() },
			};
		}

		public static Contentor Begin => new Contentor();

		public IActionCommit Execute(ActionType actionType, UserStore userStore, IActionBuilderManager actionBuilderManager,
			ProfileModel profile, DateTime executionTime)=>_factories[actionType].Commit(userStore,actionBuilderManager,profile,executionTime);
	}
}

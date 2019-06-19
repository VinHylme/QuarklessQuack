using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;

namespace Quarkless.Services.Factories
{
	public class ActionsManager
	{
		private readonly Dictionary<ActionType, ActionBuilderFactory> _factories;

		public ActionsManager()
		{
			_factories = new Dictionary<ActionType, ActionBuilderFactory>
			{
				{ ActionType.FollowUser, new FollowUserActionBuilderFactory() },
				{ ActionType.CreatePostTypeImage, new CreateImagePostActionBuilderFactory() },
				{ ActionType.CreatePostTypeVideo, new CreateVideoPostActionBuilderFactory() }
			};
		}

		public static ActionsManager Begin => new ActionsManager();

		public IActionCommit Commit(ActionType actionType, UserStore userStore, IContentManager actionBuilderManager,
			ProfileModel profile, DateTime executionTime)=>_factories[actionType].Commit(userStore,actionBuilderManager,profile,executionTime);
	}
}

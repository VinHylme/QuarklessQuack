using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
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
				{ ActionType.CreatePost, new CreatePostActionBuilderFactory() },
				{ ActionType.LikePost, new LikeMediaPostActionBuilderFactory()},
				{ ActionType.CreateCommentMedia, new CreateCommentMediaActionBuilderFactory() },
				{ ActionType.MaintainAccount, new AccountCheckerActionBuilderFactory() },
				{ ActionType.UnFollowUser, new UnFollowUserActionBuilderFactory() },
				{ ActionType.LikeComment, new LikeCommentActionBuilderFactory() }
			};
		}

		public static ActionsManager Begin => new ActionsManager();

		public IActionCommit Commit(ActionType actionType,IContentManager actionBuilderManager, IHeartbeatLogic heartbeatLogic) 
			=>_factories[actionType].Commit(actionBuilderManager,heartbeatLogic);
	}
}

using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Enums;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System.Collections.Generic;
using QuarklessLogic.Handlers.ContentInfoBuilder;
using QuarklessLogic.Handlers.RequestBuilder.Constants;

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
				{ ActionType.LikeComment, new LikeCommentActionBuilderFactory() },
				{ ActionType.SendDirectMessage, new SendDirectMessageActionBuilderFactory() }
			};
		}

		public static ActionsManager Begin => new ActionsManager();

		public IActionCommit Commit(ActionType actionType, IContentInfoBuilder actionBuilderManager, 
			IHeartbeatLogic heartbeatLogic, IUrlReader urlReader) 
			=>_factories[actionType].Commit(actionBuilderManager,heartbeatLogic, urlReader);
	}
}

﻿using System.Collections.Generic;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.Services.Automation.Factory;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Logic.Services.Automation.Factory.FactoryManager
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
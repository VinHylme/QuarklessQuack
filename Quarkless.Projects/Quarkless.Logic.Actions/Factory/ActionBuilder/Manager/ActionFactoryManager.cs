using System.Collections.Generic;
using Quarkless.Models.Actions.Factory;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Lookup.Interfaces;

namespace Quarkless.Logic.Actions.Factory.ActionBuilder.Manager
{
	public class ActionFactoryManager : IActionCommitFactory
	{
		private readonly Dictionary<ActionType, ActionBuilderFactory> _factories;

		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly ILookupLogic _lookupLogic;
		public ActionFactoryManager(IHeartbeatLogic heartbeatLogic, IContentInfoBuilder contentInfoBuilder,
			ILookupLogic lookupLogic)
		{
			_heartbeatLogic = heartbeatLogic;
			_contentInfoBuilder = contentInfoBuilder;
			_lookupLogic = lookupLogic;

			_factories = new Dictionary<ActionType, ActionBuilderFactory>
			{
				{ ActionType.FollowUser, new ExecuteFollowUserActionFactory()},
				{ ActionType.CreatePost, new ExecutePostActionFactory()},
				{ ActionType.LikePost, new ExecuteLikeMediaActionFactory()},
				{ ActionType.CreateCommentMedia, new ExecuteCommentMediaActionFactory()},
				{ ActionType.MaintainAccount, new ExecuteAccountCheckerActionFactory() },
				{ ActionType.UnFollowUser, new ExecuteUnfollowUserActionFactory() },
				{ ActionType.LikeComment, new ExecuteLikeCommentActionFactory() },
				{ ActionType.SendDirectMessage, new ExecuteDirectMessageActionFactory() },
				{ ActionType.WatchStory, new ExecuteWatchStoryActionFactory() },
				{ ActionType.ReactStory, new ExecuteReactStoryActionFactory() }
			};
		}

		public IActionCommit Create(ActionType action, in UserStoreDetails user)
			=> _factories[action].Commit(user, _contentInfoBuilder, _heartbeatLogic, _lookupLogic);
	}
}

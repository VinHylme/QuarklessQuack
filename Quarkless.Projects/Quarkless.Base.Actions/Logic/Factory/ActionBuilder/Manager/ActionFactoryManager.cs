using System.Collections.Generic;
using Quarkless.Base.Actions.Models.Factory;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Actions.Logic.Factory.ActionBuilder.Manager
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

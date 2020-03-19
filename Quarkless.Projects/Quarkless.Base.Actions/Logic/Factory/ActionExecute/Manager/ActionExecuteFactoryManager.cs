using System.Collections.Generic;
using Quarkless.Analyser;
using Quarkless.Base.Actions.Models.Factory;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Actions.Logic.Factory.ActionExecute.Manager
{
	/// <summary>
	/// Requires IInstagramAccountLogic, IApiClientContext & IResponseResolver Interface Injected
	/// </summary>
	public class ActionExecuteFactoryManager : IActionExecuteFactory
	{
		private readonly Dictionary<ActionType, ActionExecuteFactory> _factories;

		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IApiClientContext _apiClientContext;
		private readonly IResponseResolver _responseResolver;
		public ActionExecuteFactoryManager(IInstagramAccountLogic instagramAccountLogic,
			IApiClientContext clientContext, IResponseResolver responseResolver, IPostAnalyser postAnalyser)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_apiClientContext = clientContext;
			_responseResolver = responseResolver;

			_factories = new Dictionary<ActionType, ActionExecuteFactory>
			{
				{ ActionType.FollowUser, new ExecuteFollowUserActionFactory()},
				{ ActionType.CreatePost, new ExecutePostActionFactory(postAnalyser)},
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

		public IActionExecute Create(ActionType action, in UserStore user)
			=> _factories[action].Create(new Worker(_apiClientContext, _instagramAccountLogic,
				user.AccountId, user.InstagramAccountUser), _responseResolver);
	}
}

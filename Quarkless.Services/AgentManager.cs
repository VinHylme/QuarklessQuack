using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.ProfileLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class AgentRespnse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}
	public class AgentManager : IAgentManager
	{

		private readonly IProfileLogic _profileLogic;
		private readonly IContentManager _contentManager;
		private readonly IAPIClientContext _clientContext;
		private readonly IUserStoreDetails _userStoreDetails;
		public AgentManager(IProfileLogic profileLogic,
			IContentManager contentManager, IAPIClientContext clientContext,IUserStoreDetails userStoreDetails)
		{
			_profileLogic = profileLogic;
			_contentManager = contentManager;
			_clientContext = clientContext;
			_userStoreDetails = userStoreDetails;
		}
		private IAPIClientContainer GetContext(string accountId, string instagramAccountId)
		{
			return new APIClientContainer(_clientContext, accountId, instagramAccountId);
		}
	
		public async Task<AgentRespnse> StartAgent(string accountId, string instagramAccountId, string accessToken)
		{
			try { 
				var profile = await _profileLogic.GetProfile(accountId, instagramAccountId);
				if (profile == null) return null;
				_userStoreDetails.AddUpdateUser(accountId,instagramAccountId,accessToken);			
				_contentManager.SetUser(_userStoreDetails);

				var imageAction = ActionsManager.Begin.Commit(ActionType.CreatePostTypeImage, _contentManager, profile)
					.IncludeStrategy(new ImageStrategySettings
					{
						ImageStrategyType = ImageStrategyType.Default
					});

				var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, profile)
					.IncludeStrategy(new FollowStrategySettings {
						FollowStrategy = FollowStrategyType.Default,
						NumberOfActions = 1,
						OffsetPerAction = DateTimeOffset.Now.AddMinutes(2)
					});

				var likeMediaAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager, profile)
					.IncludeStrategy(new LikeStrategySettings
					{
						LikeStrategy = LikeStrategyType.Default,
						NumberOfActions = 1,
						OffsetPerAction = DateTimeOffset.Now.AddMinutes(2)
					});

				List<Chance<Action>> actionToExecute = new List<Chance<Action>>();
				actionToExecute.Add(new Chance<Action>
				{
					Object = () => imageAction.Push(new ImageActionOptions
					{
						ExecutionTime = DateTime.UtcNow.AddMinutes(1),
						ImageFetchLimit = 20
					}),
					Probability = 0.08
				});
				actionToExecute.Add(new Chance<Action>
				{
					Object = () => followAction.Push(new FollowActionOptions
					{
						FollowActionType = FollowActionType.Any,
						ExecutionTime = DateTimeOffset.Now.AddMinutes(2)
					}),
					Probability = 0.30
				});
				actionToExecute.Add(new Chance<Action>
				{
					Object = () => likeMediaAction.Push(new LikeActionOptions
					{
						LikeActionType = LikeActionType.Any,
						ExecutionTime = DateTime.Now.AddMinutes(10),
					}),
					Probability = 0.62
				});

				_ = Task.Run(() =>
				{
					while (true)
					{
						var toExe = SecureRandom.ProbabilityRoll(actionToExecute);
						toExe();
						Thread.Sleep(TimeSpan.FromSeconds(8));
					}
				});

				return new AgentRespnse()
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Started"
				};
			}
			catch(Exception ee)
			{
				return new AgentRespnse{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
	}
}

using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.ProfileLogic;
using System;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class AgentRespnse
	{

	}
	public class AgentManager : IAgentManager
	{

		private readonly IProfileLogic _profileLogic;
		private readonly IContentManager _contentManager;
		private readonly IAPIClientContext _clientContext;
		public AgentManager(IProfileLogic profileLogic,
			IContentManager contentManager, IAPIClientContext clientContext)
		{
			_profileLogic = profileLogic;
			_contentManager = contentManager;
			_clientContext = clientContext;
		}
		private IAPIClientContainer GetContext(string accountId, string instagramAccountId)
		{
			return new APIClientContainer(_clientContext, accountId, instagramAccountId);
		}
		public async Task<AgentRespnse> StartAgent(string accountId, string instagramAccountId, string accessToken)
		{
			var profile = await _profileLogic.GetProfile(accountId, instagramAccountId);
			if (profile == null) return null;

			var followAction = ActionsManager.Begin
				.Commit(ActionType.FollowUser,
				new UserStore(accountId,instagramAccountId,accessToken),
				_contentManager,profile,DateTime.UtcNow.AddSeconds(10));

			followAction.Operate();

			var likeMediaAction = ActionsManager.Begin.Commit(ActionType.LikePost,
				new UserStore(accountId,instagramAccountId,accessToken), _contentManager,profile, DateTime.UtcNow.AddSeconds(10));

			likeMediaAction.Operate();

			//var imageAction = Contentor.Begin.ExecutePost(ContentType.Image,
			//	new UserStore(accountId,instagramAccountId,accessToken),
			//	_contentManager,
			//	profile,
			//	DateTime.UtcNow.AddSeconds(10));

			//imageAction.Operate();
			//var videoAction = Contentor.Begin.ExecutePost(ContentType.Video, new UserStore(accountId, instagramAccountId, accessToken),
			//_contentBuilderManager, profile, DateTime.UtcNow.AddMinutes(1));

			//videoAction.Operate();
			return new AgentRespnse();
		}
	}
}

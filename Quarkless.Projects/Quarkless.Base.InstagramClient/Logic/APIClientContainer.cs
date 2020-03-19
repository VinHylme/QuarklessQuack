using InstagramApiSharp.API.Processors;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Logic
{
	public class ApiClientContainer : IApiClientContainer
	{
		private InstagramAccountFetcherResponse Context { get; set; }
		private readonly IApiClientContext _clientContext;
		private readonly IUserContext _userContext;
		
		public ApiClientContainer(IApiClientContext clientContext, string userId, string instaId)
		{
			_clientContext = clientContext;
			Context = _clientContext.Create(userId, instaId).Result;
		}

		public ApiClientContainer(IApiClientContext clientContext, IUserContext userContext)
		{
			_clientContext = clientContext;
			_userContext = userContext;
			if (userContext?.FocusInstaAccount != null)
				Context = _clientContext.Create(userContext.CurrentUser, userContext.FocusInstaAccount).Result;
		}

		#region Getters
		public InstagramAccountFetcherResponse GetContext => Context;
		public IInstaClient EmpClientWithProxy (ProxyModel model, bool genDevice = false) 
			=> _clientContext.EmptyClientWithProxy(model, genDevice);

		public IInstaClient EmptyClient => _clientContext.EmptyClient;

		public IDiscoverProcessor Discover => Context.Container.ActionClient.DiscoverProcessor;

		public ICollectionProcessor Collections => Context.Container.ActionClient.CollectionProcessor;

		public IFeedProcessor Feeds => Context.Container.ActionClient.FeedProcessor;

		public IMediaProcessor Media => Context.Container.ActionClient.MediaProcessor;

		public IUserProcessor User => Context.Container.ActionClient.UserProcessor;

		public IAccountProcessor Account => Context.Container.ActionClient.AccountProcessor;

		public ILocationProcessor Location => Context.Container.ActionClient.LocationProcessor;

		public ICommentProcessor Comment => Context.Container.ActionClient.CommentProcessor;

		public IHashtagProcessor Hashtag => Context.Container.ActionClient.HashtagProcessor;

		public IBusinessProcessor Business => Context.Container.ActionClient.BusinessProcessor;

		public ILiveProcessor Live => Context.Container.ActionClient.LiveProcessor;

		public IMessagingProcessor Messaging => Context.Container.ActionClient.MessagingProcessor;

		public IWebProcessor Web => Context.Container.ActionClient.WebProcessor;

		public IShoppingProcessor Shopping => Context.Container.ActionClient.ShoppingProcessor;

		public IStoryProcessor Story => Context.Container.ActionClient.StoryProcessor;

		public ITVProcessor Tv => Context.Container.ActionClient.TVProcessor;
		#endregion
	}
}

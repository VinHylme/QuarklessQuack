using InstagramApiSharp.API.Processors;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;

namespace Quarkless.Logic.InstagramClient
{
	public class ApiClientContainer : IApiClientContainer
	{
		private ContextContainer Context { get; set; }
		private readonly IApiClientContext _clientContext;
		private readonly IUserContext _userContext;
		public ApiClientContainer(IApiClientContext clientContext, IUserContext userContext)
		{
			_clientContext = clientContext;
			_userContext = userContext;
			if (userContext?.FocusInstaAccount != null)
				this.Context = _clientContext.Create(userContext.CurrentUser, userContext.FocusInstaAccount).Result;
		}
		public ApiClientContainer(IApiClientContext clientContext, string userId, string instaId)
		{
			_clientContext = clientContext;
			this.Context = _clientContext.Create(userId, instaId).Result;
		}

		public ContextContainer GetContext => this.Context;

		public IInstaClient EmptyClient => _clientContext.EmptyClient;

		public IInstaClient EmpClientWithProxy (ProxyModel model, bool genDevice = false) 
			=> _clientContext.EmptyClientWithProxy(model, genDevice);

		public IDiscoverProcessor Discover => Context.ActionClient.DiscoverProcessor;

		public ICollectionProcessor Collections => Context.ActionClient.CollectionProcessor;

		public IFeedProcessor Feeds => Context.ActionClient.FeedProcessor;

		public IMediaProcessor Media => Context.ActionClient.MediaProcessor;

		public IUserProcessor User => Context.ActionClient.UserProcessor;

		public IAccountProcessor Account => Context.ActionClient.AccountProcessor;

		public ILocationProcessor Location => Context.ActionClient.LocationProcessor;

		public ICommentProcessor Comment => Context.ActionClient.CommentProcessor;

		public IHashtagProcessor Hashtag => Context.ActionClient.HashtagProcessor;

		public IBusinessProcessor Business => Context.ActionClient.BusinessProcessor;

		public ILiveProcessor Live => Context.ActionClient.LiveProcessor;

		public IMessagingProcessor Messaging => Context.ActionClient.MessagingProcessor;

		public IWebProcessor Web => Context.ActionClient.WebProcessor;

		public IShoppingProcessor Shopping => Context.ActionClient.ShoppingProcessor;

		public IStoryProcessor Story => Context.ActionClient.StoryProcessor;

		public ITVProcessor Tv => Context.ActionClient.TVProcessor;
	}
}

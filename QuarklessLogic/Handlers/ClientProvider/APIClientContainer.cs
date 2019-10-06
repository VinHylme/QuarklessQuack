using InstagramApiSharp.API.Processors;
using QuarklessContexts.Contexts;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using QuarklessContexts.Models.Proxies;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public class APIClientContainer : IAPIClientContainer
	{
		private ContextContainer Context { get; set; }
		private readonly IAPIClientContext _clientContext;
		private readonly IUserContext _userContext;
		public APIClientContainer(IAPIClientContext clientContext, IUserContext userContext)
		{
			_clientContext = clientContext;
			_userContext = userContext;
			if(userContext?.FocusInstaAccount != null)
				this.Context = _clientContext.Create(userContext.CurrentUser, userContext.FocusInstaAccount).GetAwaiter().GetResult();
		}
		public APIClientContainer(IAPIClientContext clientContext, string userId, string instaId)
		{
			_clientContext = clientContext;
			this.Context = _clientContext.Create(userId, instaId).GetAwaiter().GetResult();
		}

		public ContextContainer GetContext => this.Context;

		public InstaClient EmptyClient => _clientContext.EmptyClient;

		public InstaClient EmpClientWithProxy (ProxyModel model, bool genDevice = false) => _clientContext.EmptyClientWithProxy(model, genDevice);

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

		public ITVProcessor TV => Context.ActionClient.TVProcessor;
	}
}

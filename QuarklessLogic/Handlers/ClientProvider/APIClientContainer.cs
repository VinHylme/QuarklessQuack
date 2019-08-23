using InstagramApiSharp.API.Processors;
using QuarklessContexts.Contexts;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

		public ContextContainer GetContext
		{
			get
			{
				return this.Context;
			}
		}
		public InstaClient EmptyClient
		{
			get
			{
				return _clientContext.EmptyClient;
			}
		}

		public IDiscoverProcessor Discover
		{
			get
			{
				return Context.ActionClient.DiscoverProcessor;
			}
		}
		public ICollectionProcessor Collections
		{
			get
			{
				return Context.ActionClient.CollectionProcessor;
			}
		}
		public IFeedProcessor Feeds
		{
			get
			{
				return Context.ActionClient.FeedProcessor;
			}
		}
		public IMediaProcessor Media
		{
			get
			{
				return Context.ActionClient.MediaProcessor;
			}
		}
		public IUserProcessor User
		{
			get
			{
				return Context.ActionClient.UserProcessor;
			}
		}
		public IAccountProcessor Account
		{
			get
			{
				return Context.ActionClient.AccountProcessor;
			}
		}
		public ILocationProcessor Location
		{
			get
			{
				return Context.ActionClient.LocationProcessor;
			}
		}
		public ICommentProcessor Comment
		{
			get
			{
				return Context.ActionClient.CommentProcessor;
			}
		}
		public IHashtagProcessor Hashtag
		{
			get
			{
				return Context.ActionClient.HashtagProcessor;
			}
		}
		public IBusinessProcessor Business
		{
			get
			{
				return Context.ActionClient.BusinessProcessor;
			}
		}
		public ILiveProcessor Live
		{
			get
			{
				return Context.ActionClient.LiveProcessor;
			}
		}
		public IMessagingProcessor Messaging
		{
			get
			{
				return Context.ActionClient.MessagingProcessor;
			}
		}
		public IWebProcessor Web
		{
			get
			{
				return Context.ActionClient.WebProcessor;
			}
		}
		public IShoppingProcessor Shopping
		{
			get
			{
				return Context.ActionClient.ShoppingProcessor;
			}
		}
		public IStoryProcessor Story
		{
			get
			{
				return Context.ActionClient.StoryProcessor;
			}
		}
		public ITVProcessor TV
		{
			get
			{
				return Context.ActionClient.TVProcessor;
			}
		}

	}
}

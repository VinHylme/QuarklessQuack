﻿using InstagramApiSharp.API.Processors;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public interface IAPIClientContainer
	{
		IAccountProcessor Account { get; }
		IBusinessProcessor Business { get; }
		ICollectionProcessor Collections { get; }
		ICommentProcessor Comment { get; }
		IDiscoverProcessor Discover { get; }
		InstaClient EmptyClient { get; }
		IFeedProcessor Feeds { get; }
		ContextContainer GetContext { get; }
		IHashtagProcessor Hashtag { get; }
		ILiveProcessor Live { get; }
		ILocationProcessor Location { get; }
		IMediaProcessor Media { get; }
		IMessagingProcessor Messaging { get; }
		IShoppingProcessor Shopping { get; }
		IStoryProcessor Story { get; }
		ITVProcessor TV { get; }
		IUserProcessor User { get; }
		IWebProcessor Web { get; }
	}
}
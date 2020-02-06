using InstagramApiSharp.API.Processors;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient.Interfaces
{
	public interface IApiClientContainer
	{
		InstagramAccountFetcherResponse GetContext { get; }
		IInstaClient EmpClientWithProxy(ProxyModel model, bool genDevice = false);
		IInstaClient EmptyClient { get; }
		IAccountProcessor Account { get; }
		IBusinessProcessor Business { get; }
		ICollectionProcessor Collections { get; }
		ICommentProcessor Comment { get; }
		IDiscoverProcessor Discover { get; }
		IFeedProcessor Feeds { get; }
		IHashtagProcessor Hashtag { get; }
		ILiveProcessor Live { get; }
		ILocationProcessor Location { get; }
		IMediaProcessor Media { get; }
		IMessagingProcessor Messaging { get; }
		IShoppingProcessor Shopping { get; }
		IStoryProcessor Story { get; }
		ITVProcessor Tv { get; }
		IUserProcessor User { get; }
		IWebProcessor Web { get; }
	}
}
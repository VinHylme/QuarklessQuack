using InstagramApiSharp.API.Processors;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient.Interfaces
{
	public interface IApiClientContainer
	{
		IAccountProcessor Account { get; }
		IBusinessProcessor Business { get; }
		ICollectionProcessor Collections { get; }
		ICommentProcessor Comment { get; }
		IDiscoverProcessor Discover { get; }
		IInstaClient EmptyClient { get; }
		IInstaClient EmpClientWithProxy(ProxyModel model, bool genDevice = false);
		IFeedProcessor Feeds { get; }
		ContextContainer GetContext { get; }
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
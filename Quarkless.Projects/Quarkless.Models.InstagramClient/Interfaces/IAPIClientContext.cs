using System.Threading.Tasks;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient.Interfaces
{
	public interface IApiClientContext
	{
		Task<ContextContainer> Create(string userId, string instaId);
		IInstaClient EmptyClient {get; }
		IInstaClient EmptyClientWithProxy(ProxyModel model, bool genDevice = false);
	}
}
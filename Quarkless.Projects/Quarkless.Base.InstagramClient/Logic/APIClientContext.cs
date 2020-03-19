using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramClient.Models;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Logic
{
	public class ApiClientContext : ApiClientContextFactory, IApiClientContext
	{
		private readonly IClientContextProvider _clientContextProvider;
		public ApiClientContext(IClientContextProvider clientContextProvider)
		{
			_clientContextProvider = clientContextProvider;
		}

		public override async Task<InstagramAccountFetcherResponse> Create(string userId, string instaId)
		{
			return await _clientContextProvider.Get(userId, instaId);
		}

		public IInstaClient EmptyClient => _clientContextProvider.InitialClientGenerate();

		public IInstaClient EmptyClientWithUser(UserSessionData userData)
			=> _clientContextProvider.InitialClientGenerate(userData);

		public IInstaClient EmptyClientWithProxy (ProxyModel model, bool genDevice = false) =>
			_clientContextProvider.InitialClientGenerateWithProxy(model, genDevice);
	}
}

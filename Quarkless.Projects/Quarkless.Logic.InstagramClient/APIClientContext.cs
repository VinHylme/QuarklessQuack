using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;

namespace Quarkless.Logic.InstagramClient
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

using System.Threading.Tasks;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public class APIClientContext : APIClientContextFactory, IAPIClientContext
	{
		private readonly IClientContextProvider _clientContextProvider;
		public APIClientContext(IClientContextProvider clientContextProvider)
		{
			_clientContextProvider = clientContextProvider;
		}

		public override async Task<ContextContainer> Create(string userId, string instaId)
		{
			return await _clientContextProvider.Get(userId, instaId);
		}

		public InstaClient EmptyClient
		{
			get
			{
				return _clientContextProvider.InitialClientGenerate();
			}
		}
	}
}

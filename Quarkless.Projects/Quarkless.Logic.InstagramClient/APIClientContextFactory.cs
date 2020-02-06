using System.Threading.Tasks;
using Quarkless.Models.InstagramClient;

namespace Quarkless.Logic.InstagramClient
{
	public abstract class ApiClientContextFactory
	{
		public abstract Task<InstagramAccountFetcherResponse> Create(string userId, string InstaId);
	}
}

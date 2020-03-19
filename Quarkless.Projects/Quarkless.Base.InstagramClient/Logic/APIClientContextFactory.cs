using System.Threading.Tasks;
using Quarkless.Base.InstagramClient.Models;

namespace Quarkless.Base.InstagramClient.Logic
{
	public abstract class ApiClientContextFactory
	{
		public abstract Task<InstagramAccountFetcherResponse> Create(string userId, string InstaId);
	}
}

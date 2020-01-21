using System.Threading.Tasks;
using Quarkless.Models.InstagramClient;

namespace Quarkless.Logic.InstagramClient
{
	public abstract class ApiClientContextFactory
	{
		public abstract Task<ContextContainer> Create(string userId, string InstaId);
	}
}

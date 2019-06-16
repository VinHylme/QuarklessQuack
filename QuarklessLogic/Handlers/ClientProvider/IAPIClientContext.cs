using System.Threading.Tasks;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public interface IAPIClientContext
	{
		Task<ContextContainer> Create(string userId, string instaId);
		InstaClient EmptyClient {get; }
	}
}
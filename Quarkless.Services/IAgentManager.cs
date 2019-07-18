using QuarklessContexts.Models.AgentModels;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public interface IAgentManager
	{
		Task<AgentResponse> StartAgent(string accountId, string instagramAccountId, string accessToken);
		Task<AgentResponse> StopAgent(string accountId, string instagramAccountId);
	}
}
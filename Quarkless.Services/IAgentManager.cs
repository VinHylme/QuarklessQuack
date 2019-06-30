using System.Threading.Tasks;

namespace Quarkless.Services
{
	public interface IAgentManager
	{
		Task<AgentRespnse> StartAgent(string accountId, string instagramAccountId, string accessToken);
		AgentRespnse StopAgent(string instagramAccountId);
	}
}
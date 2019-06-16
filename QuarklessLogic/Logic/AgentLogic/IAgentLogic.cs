using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AgentLogic
{
	public interface IAgentLogic
	{
		Task<bool> Begin(string user, string instagramAccountId);
		Task<bool> Stop(string user, string instagramAccountId);
		Task<bool> StartScrape(string user, string instagramAccountId);
	}
}
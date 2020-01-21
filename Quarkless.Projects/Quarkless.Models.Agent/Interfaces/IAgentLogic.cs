using Quarkless.Models.InstagramAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Agent.Interfaces
{
	public interface IAgentLogic
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts();
		Task<IEnumerable<ShortInstagramAccountModel>> GetAllAccounts(int type = 0);
		Task<AgentResponse> Start(string accountId, string instagramAccountId);
		Task<AgentResponse> Stop(string accountId, string instagramAccountId);
	}
}
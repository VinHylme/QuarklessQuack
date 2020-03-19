using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Enums;

namespace Quarkless.Base.Agent.Models.Interfaces
{
	public interface IAgentLogic
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts();
		Task<IEnumerable<ShortInstagramAccountModel>> GetAllAccounts(int type = 0);
		Task<AgentResponse> Start(string accountId, string instagramAccountId);
		Task<AgentResponse> Stop(string accountId, string instagramAccountId);
		Task<AgentResponse> ChangeAgentState(string accountId, string instagramAccountId, AgentState state);
		Task<ShortInstagramAccountModel> GetAccount(string accountId, string instagramAccountId);
	}
}
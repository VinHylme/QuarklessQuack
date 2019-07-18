using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.AgentModels;
using QuarklessContexts.Models.InstagramAccounts;

namespace QuarklessLogic.ServicesLogic.AgentLogic
{
	public interface IAgentLogic
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts();
		Task<AgentResponse> Start(string accountId, string instagramAccountId);
		Task<AgentResponse> Stop(string accountId, string instagramAccountId);
		Task<bool> UpdateAgentStateForUser(string accountId, string instagramAccountId, AgentState newState, ActionStates actionStates);
	}
}
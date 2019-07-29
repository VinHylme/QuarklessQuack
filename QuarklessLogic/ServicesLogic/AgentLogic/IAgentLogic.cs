using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.AgentModels;
using QuarklessContexts.Models.InstagramAccounts;

namespace QuarklessLogic.ServicesLogic.AgentLogic
{
	public interface IAgentLogic
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts();
		Task<IEnumerable<ShortInstagramAccountModel>> GetAllAccounts(int type = 0);
		Task<AgentResponse> Start(string accountId, string instagramAccountId);
		Task<AgentResponse> Stop(string accountId, string instagramAccountId);
	}
}
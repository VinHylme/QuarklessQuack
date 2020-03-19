using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.InstagramAccounts.Models.Interfaces
{
	public interface IInstagramAccountRepository
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type);
		Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsFull(int type);
		Task<ResultCarrier<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string accountId, int type);
		Task<ResultCarrier<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount);
		Task<bool> AddBlockedAction(string instagramAccountId, ActionType actionType, DateTime? blockFor = null);
		Task<bool> RemoveBlockedAction(string instagramAccountId, ActionType actionType);
		Task<bool> ClearCacheData(string accountId, string instagramAccountId, StateData state);
		Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<ResultCarrier<StateData>> GetInstagramAccountStateData(string accountId, string instagramAccountId);
		Task EmptyChallengeInfo(string instagramAccountId);
		Task<bool> RemoveInstagramAccount(string id);
		Task UpdateMultipleUserAgentStates(AgentState state, int accountType = 1, AgentState targetedStates = AgentState.Working);
	}
}

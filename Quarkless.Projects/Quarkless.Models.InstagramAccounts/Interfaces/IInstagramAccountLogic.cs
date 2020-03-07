using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.InstagramAccounts.Enums;

namespace Quarkless.Models.InstagramAccounts.Interfaces
{
	public interface IInstagramAccountLogic
	{
		Task<bool> RemoveBlockedAction(string instagramAccountId, ActionType actionType);
		Task<bool> AddBlockedAction(string instagramAccountId, ActionType actionType, DateTime? blockFor = null);
		Task<InstagramAccountModel> InsertInstagramAccount(string accountId, AddInstagramAccountRequest addRequest);
		Task<ResultCarrier<AddInstagramAccountResponse>> AddInstagramAccount(string accountId, AddInstagramAccountRequest addInstagram);
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type);
		Task<bool> ClearCacheData(string accountId, string instagramAccountId);
		Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsFull(int type);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountsOfUser(string accountId, int type = 0);
		Task<ShortInstagramAccountModel> GetInstagramAccountShort(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<long?> PartialUpdateInstagramAccount(string accountId, string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<StateData> GetInstagramAccountStateData(string accountId, string instagramAccountId);
		Task EmptyChallengeInfo(string instagramAccountId);
		Task<bool> RemoveInstagramAccount(string instagramAccountId);
		Task UpdateAgentStates(AgentState state, int accountType = 1, AgentState targetedStates = AgentState.Working);
	}
}

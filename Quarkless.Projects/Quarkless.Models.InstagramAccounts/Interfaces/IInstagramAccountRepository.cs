using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Models.InstagramAccounts.Interfaces
{
	public interface IInstagramAccountRepository
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type);
		Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsFull(int type);
		Task<ResultCarrier<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string accountId, int type);
		Task<ResultCarrier<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount);
		Task<bool> AddBlockedAction(string instagramAccountId, ActionType actionType);
		Task<bool> RemoveBlockedAction(string instagramAccountId, ActionType actionType);
		Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<ResultCarrier<StateData>> GetInstagramAccountStateData(string accountId, string instagramAccountId);
		Task EmptyChallengeInfo(string instagramAccountId);
		Task<bool> RemoveInstagramAccount(string id);
	}
}

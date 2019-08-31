using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.InstagramAccountLogic
{
	public interface IInstagramAccountLogic
	{
		Task<ResultCarrier<AddInstagramAccountResponse>> AddInstagramAccount(string accountId, StateData state, AddInstagramAccountRequest addInstagram);
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountsOfUser(string accountId, int type = 0);
		Task<ShortInstagramAccountModel> GetInstagramAccountShort(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<long?> PartialUpdateInstagramAccount(string accountId,string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<StateData> GetInstagramAccountStateData(string accountId, string InstagramAccountId);
		Task EmptyChallengeInfo(string instagramAccountId);
	}
}
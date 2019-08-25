using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.InstagramAccountRepository
{
	public interface IInstagramAccountRepository
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts();
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type);
		Task<ResultCarrier<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string accountId, int type);
		Task<ResultCarrier<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount);
		Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<ResultCarrier<StateData>> GetInstagramAccountStateData(string accountId, string instagramAccountId);
		Task EmptyChallengeInfo(string instagramAccountId);
	}
}
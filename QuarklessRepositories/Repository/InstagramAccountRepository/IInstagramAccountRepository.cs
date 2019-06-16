﻿using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.InstagramAccountRepository
{
	public interface IInstagramAccountRepository
	{
		Task<ResultBase<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string accountId, int type);
		Task<ResultBase<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount);
		Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<ResultBase<StateData>> GetInstagramAccountStateData(string accountId, string instagramAccountId);
	}
}
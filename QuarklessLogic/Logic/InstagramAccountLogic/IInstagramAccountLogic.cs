﻿using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.InstagramAccountLogic
{
	public interface IInstagramAccountLogic
	{
		Task<ResultBase<bool>> AddInstagramAccount(string accountId, StateData state,AddInstagramAccountRequest addInstagram);
		Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsOfUser(string accountId, int type = 0);
		Task<InstagramAccountModel> GetInstagramAccount(string accountId, string instagramAccountId);
		Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel);
		Task<StateData> GetInstagramAccountStateData(string accountId, string InstagramAccountId);
	}
}
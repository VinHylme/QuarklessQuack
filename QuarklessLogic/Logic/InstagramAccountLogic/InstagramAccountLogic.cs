using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessRepositories.InstagramAccountRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.InstagramAccountLogic
{
	public class InstagramAccountLogic : IInstagramAccountLogic
	{
		private readonly IProxyLogic _proxyLogic;
		private readonly IInstagramAccountRepository _instagramAccountRepository;
		public InstagramAccountLogic(IProxyLogic proxyLogic, IInstagramAccountRepository instagramAccountRepository)
		{
			_proxyLogic = proxyLogic;
			_instagramAccountRepository = instagramAccountRepository;
		}

		public async Task<ResultBase<bool>> AddInstagramAccount(string accountId, StateData state, AddInstagramAccountRequest addInstagram)
		{
			ResultBase<bool> @Result = new ResultBase<bool>();
				try {
					var device = state.DeviceInfo.DeviceModel;
					var instamodel = new InstagramAccountModel
					{
						Device = device,
						State = state,
						AccountId = accountId,
						FollowersCount = null,
						FollowingCount = null,
						Password = addInstagram.Password,
						Username = addInstagram.Username,
						TotalLikes = null,
						TotalPostsCount = null,
						Type = addInstagram.Type
					};
					var result = await _instagramAccountRepository.AddInstagramAccount(instamodel);
					if(result!=null)
					{
						var assign = await _proxyLogic.AssignProxy(new AssignedTo { Account_Id = result.AccountId, InstaId = result._id.ToString()});
						if (assign)
						{
							Result.Results = true;
							return Result;

						}
						Result.Message = $"Failed to assign proxy to user {result.AccountId}, instagram account {result.Username}";
					}
					Result.Message = $"Failed to add instagram user of {result.AccountId}, instagram account {result.Username}";
					return null;
				}
				catch(Exception ee)
				{
					Result.Message = $"Exepction trying to add user: {addInstagram.Username}, error: {ee.Message}";
					return Result;
				}	
		}

		public async Task<StateData> GetInstagramAccountStateData(string accountId, string InstagramAccountId)
		{
			try
			{
				var state  = await _instagramAccountRepository.GetInstagramAccountStateData(accountId, InstagramAccountId);
				if (state.Results != null)
				{
					return state.Results;
				}
				return null;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public async Task<InstagramAccountModel> GetInstagramAccount(string accountId, string instagramAccountId)
		{
			try
			{
				var account = await _instagramAccountRepository.GetInstagramAccount(accountId, instagramAccountId);

				if (account.Results != null)
				{
					return account.Results;
				}
				return null;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public async Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsOfUser(string accountId, int type = 0)
		{
			try
			{
				var accounts = await _instagramAccountRepository.GetInstagramAccountsOfUser(accountId, type);
				if(accounts.Results!= null)
				{
					return accounts.Results;
				}
				else
				{
					return null;
				}
			}
			catch (Exception ee)
			{
				return null;
			}
		}
		
		public async Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel)
		{
			return await _instagramAccountRepository.PartialUpdateInstagramAccount(instagramAccountId, instagramAccountModel);
		}

	}
}

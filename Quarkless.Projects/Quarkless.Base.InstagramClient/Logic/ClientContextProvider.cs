using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.Proxy.Models.Interfaces;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.InstagramClient.Logic
{
	public class ClientContextProvider : IClientContextProvider
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IInstaClient _instaClient;

		public ClientContextProvider(IInstagramAccountLogic instagramAccountLogic,
			IProfileLogic profileLogic, IProxyLogic proxyLogic, 
			IReportHandler reportHandler)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_reportHandler = reportHandler;
			_instaClient = new InstaClient(null); // giving it a null value will automatically instantiate the object
			_reportHandler.SetupReportHandler("/Logic/ClientContextProvider");
		}

		public async Task<InstagramAccountFetcherResponse> Get(string accId, string insAccId)
		{
			return await GetClient(accId, insAccId);
		}
		public IInstaClient InitialClientGenerate()
		{
			try
			{
				return _instaClient.Empty();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee).GetAwaiter().GetResult();
				return null;
			}
		}
		public IInstaClient InitialClientGenerate(UserSessionData userData)
		{
			try
			{
				return _instaClient.Empty(userData);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee).GetAwaiter().GetResult();
				return null;
			}
		}
		public IInstaClient InitialClientGenerateWithProxy(ProxyModel model, bool genDevice = false) 
			=> _instaClient.Empty(model, genDevice);


		private async Task<InstagramAccountFetcherResponse> GetClient(string accountId, string instagramAccountId)
		{
			try
			{
				var instagramAccount = await _instagramAccountLogic.GetInstagramAccount(accountId, instagramAccountId);

				if (instagramAccount == null)
					return new InstagramAccountFetcherResponse
					{
						Container = null,
						SuccessfullyRetrieved = false,
						Errors = new ErrorResponse
						{
							Message = "Could not find instagram Account"
						}
					};
				
				var profileOfInstagramAccountModel = await _profileLogic.GetProfile(accountId, instagramAccountId);
				var proxyOfInstagramAccountModel = await _proxyLogic.GetProxyAssigned(accountId, instagramAccountId);

				var clientResponse = _instaClient.GetClientFromModel(new InstagramClientAccount
				{
					InstagramAccount = instagramAccount,
					Profile = profileOfInstagramAccountModel,
					Proxy = proxyOfInstagramAccountModel
				});

				var stateJson = await clientResponse.Value.GetStateDataFromString();
				var instagramAccountDetails = new ShortInstagramAccountModel
				{
					AccountId = instagramAccount.AccountId,
					AgentState = instagramAccount.AgentState,
					DateAdded = instagramAccount.DateAdded,
					LastPurgeCycle = instagramAccount.LastPurgeCycle,
					FollowersCount = instagramAccount.FollowersCount,
					FollowingCount = instagramAccount.FollowingCount,
					Id = instagramAccount._id,
					TotalPostsCount = instagramAccount.TotalPostsCount,
					Username = instagramAccount.Username,
					Email = instagramAccount.Email,
					SleepTimeRemaining = instagramAccount.SleepTimeRemaining,
					FullName = instagramAccount.FullName,
					PhoneNumber = instagramAccount.PhoneNumber,
					ProfilePicture = instagramAccount.ProfilePicture,
					UserBiography = instagramAccount.UserBiography,
					UserLimits = instagramAccount.UserLimits,
					Location = instagramAccount.Location,
					IsBusiness = instagramAccount.IsBusiness,
					ChallengeInfo = instagramAccount.ChallengeInfo,
					BlockedActions = instagramAccount.BlockedActions,
					Type = instagramAccount.Type,
					UserId = instagramAccount.UserId
				};

				if (!string.IsNullOrEmpty(stateJson))
				{
					try
					{
						var state = JsonConvert.DeserializeObject<StateData>(stateJson);

						if(string.IsNullOrEmpty(instagramAccountDetails.FullName))
							instagramAccountDetails.FullName = state.UserSession?.LoggedInUser?.FullName;

						if(string.IsNullOrEmpty(instagramAccountDetails.ProfilePicture))
							instagramAccountDetails.ProfilePicture = state.UserSession?.LoggedInUser?.ProfilePicture;

						if (!state.SameAs(instagramAccount.State))
						{
							await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
								new InstagramAccountModel
								{
									State = state,
									ProfilePicture = instagramAccountDetails.ProfilePicture,
									FullName = instagramAccountDetails.FullName,
								});
						}
					}
					catch(Exception err)
					{
						Console.WriteLine($"error parsing state data: {err.Message}");
					}
				}

				var container = new ContextContainer
				{
					ActionClient = clientResponse.Value.ReturnClient,
					InstaClient = clientResponse.Value,
					InstagramAccount = instagramAccountDetails,
					Profile = profileOfInstagramAccountModel,
					Proxy = proxyOfInstagramAccountModel
				};

				if (!clientResponse.Succeeded)
				{
					return new InstagramAccountFetcherResponse
					{
						Container = container,
						Errors = new ErrorResponse
						{
							Message = "Failed to login to account"
						},
						Response = clientResponse,
						SuccessfullyRetrieved = false
					};
				}

				return new InstagramAccountFetcherResponse
				{
					Container = container,
					Response = clientResponse,
					SuccessfullyRetrieved = true
				};
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return new InstagramAccountFetcherResponse
				{
					Container = null,
					Errors = new ErrorResponse
					{
						Message = err.Message,
						Exception = err
					},
					SuccessfullyRetrieved = false
				};
			}
		}
	}
}
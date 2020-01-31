using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using Quarkless.Logic.Proxy;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Logic.InstagramClient
{
	public static class MiniExtensionHelper
	{
		public static bool SameAs(this StateData self, StateData target)
		{
			try
			{
				if (self == null || target == null)
					return false;

				var isDeviceSame = JsonConvert.SerializeObject(self.DeviceInfo)
					.Equals(JsonConvert.SerializeObject(target.DeviceInfo));

				var isAuthSame = self.IsAuthenticated.Equals(target.IsAuthenticated);

				var isRawCookiesSame = JsonConvert.SerializeObject(self.RawCookies)
					.Equals(JsonConvert.SerializeObject(target.RawCookies));

				var isUserSessionSame = JsonConvert.SerializeObject(self.UserSession)
					.Equals(JsonConvert.SerializeObject(target.UserSession));

				return isUserSessionSame && isAuthSame && isDeviceSame && isRawCookiesSame;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}
	}
	public class ClientContextProvider : IClientContextProvider
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IInstaClient _instaClient;

		public ClientContextProvider(IInstagramAccountLogic instagramAccountLogic,
			IProfileLogic profileLogic, IProxyLogic proxyLogic, 
			IReportHandler reportHandler, IInstaClient instaClient)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_reportHandler = reportHandler;
			_instaClient = instaClient;
			_reportHandler.SetupReportHandler("/Logic/ClientContextProvider");
		}

		public async Task<ContextContainer> Get(string accId, string insAccId)
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

		private async Task<ContextContainer> GetClient(string accountId, string instagramAccountId)
		{
			try
			{
				var instagramAccount = await _instagramAccountLogic.GetInstagramAccount(accountId, instagramAccountId);
				
				if (instagramAccount == null)
					return null;
				
				var profileOfInstagramAccountModel = await _profileLogic.GetProfile(accountId, instagramAccountId);
				var proxyOfInstagramAccountModel = await _proxyLogic.GetProxyAssigned(accountId, instagramAccountId);

				var client = _instaClient.GetClientFromModel(new InstagramClientAccount
				{
					InstagramAccount = instagramAccount,
					Profile = profileOfInstagramAccountModel,
					Proxy = proxyOfInstagramAccountModel
				});

				var stateExtracted = JsonConvert.DeserializeObject<StateData>(await client.GetStateDataFromString());
				
				if (client?.ReturnClient != null)
				{
					if (stateExtracted.SameAs(instagramAccount.State))
						return new ContextContainer
						{
							ActionClient = client.ReturnClient,
							InstagramAccount = new ShortInstagramAccountModel
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
							},
							Profile = profileOfInstagramAccountModel,
							Proxy = proxyOfInstagramAccountModel,
							InstaClient = client
						};

					instagramAccount.FullName = stateExtracted.UserSession.LoggedInUser.FullName;
					instagramAccount.ProfilePicture = stateExtracted.UserSession.LoggedInUser.ProfilePicture 
						?? stateExtracted.UserSession.LoggedInUser.ProfilePicUrl;

					await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId, 
						new InstagramAccountModel
					{
						State = stateExtracted,
					});

					return new ContextContainer
					{
						ActionClient = client.ReturnClient,
						InstagramAccount = new ShortInstagramAccountModel
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
						},
						Profile = profileOfInstagramAccountModel,
						Proxy = proxyOfInstagramAccountModel,
						InstaClient = client
					};
				}
				await _reportHandler.MakeReport($"GetClientFor user: {accountId}, insta: {instagramAccountId} failed, client returned nothing");
				return null;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
	}
}
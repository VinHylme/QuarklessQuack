using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.Proxy.Logic;
using Quarkless.Base.Proxy.Models.Enums;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Events.Interfaces;
using Quarkless.Events.Models;
using Quarkless.Events.Models.PublishObjects;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;
using InstagramAccountModel = Quarkless.Base.InstagramAccounts.Models.InstagramAccountModel;

namespace Quarkless.Base.InstagramAccounts.Logic
{
	public class InstagramAccountLogic : IInstagramAccountLogic
	{
		private readonly IInstagramAccountRepository _instagramAccountRepository;
		private readonly IEventPublisher _publisher;
		private readonly IReportHandler _reportHandler;
		public InstagramAccountLogic(IInstagramAccountRepository instagramAccountRepository,
		IEventPublisher publisher, IReportHandler reportHandler)
		{
			_publisher = publisher;
			_instagramAccountRepository = instagramAccountRepository;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("InstagramAccountLogic");
		}

		public async Task<InstagramAccountModel> InsertInstagramAccount(string accountId,
			AddInstagramAccountRequest addRequest)
		{
			var instagramAccountModel = new InstagramAccountModel
			{
				DeviceDetail = AndroidDeviceGenerator.GetRandomAndroidDevice(),
				AccountId = accountId,
				Password = addRequest.Password,
				Username = addRequest.Username,
				Type = addRequest.Type,
				AgentState = (int)AgentState.NotStarted,
				DateAdded = DateTime.UtcNow,
			};
			var results = await _instagramAccountRepository.AddInstagramAccount(instagramAccountModel);
			return results;
		}

		public async Task<ResultCarrier<AddInstagramAccountResponse>> AddInstagramAccount(string accountId,
			AddInstagramAccountRequest addInstagram)
		{
			var resultCarrier = new ResultCarrier<AddInstagramAccountResponse>();
			ProxyModelShort useProxyFromUser = null;

			if (string.IsNullOrEmpty(addInstagram.Username) || string.IsNullOrEmpty(addInstagram.Password))
			{
				resultCarrier.IsSuccessful = false;
				resultCarrier.Info = new ErrorResponse
				{
					Message = "Username or password must be provided"
				};
				return resultCarrier;
			}

			if (addInstagram.ProxyDetail == null)
			{
				if (addInstagram.Location == null
					|| (!addInstagram.EnableAutoLocate && addInstagram.Location?.Address == null))
				{
					resultCarrier.IsSuccessful = false;
					resultCarrier.Info = new ErrorResponse
					{
						Message = "Location needs to be provided"
					};
					return resultCarrier;
				}
			}
			else //use user's proxy specified
			{
				if (string.IsNullOrEmpty(addInstagram.ProxyDetail.HostAddress))
				{
					resultCarrier.IsSuccessful = false;
					resultCarrier.Info = new ErrorResponse
					{
						Message = "Proxy address needs to be provided"
					};
					return resultCarrier;
				}

				if (int.TryParse(addInstagram.ProxyDetail.Port, out var port) && (port <= 0 || port > 65535))
				{
					resultCarrier.IsSuccessful = false;
					resultCarrier.Info = new ErrorResponse
					{
						Message = "Please enter a valid port"
					};
					return resultCarrier;
				}

				//test proxy connectivity
				useProxyFromUser = new ProxyModelShort
				{
					HostAddress = addInstagram.ProxyDetail.HostAddress,
					Port = port,
					NeedServerAuth = !string.IsNullOrEmpty(addInstagram.ProxyDetail.Username),
					Username = addInstagram.ProxyDetail.Username,
					Password = addInstagram.ProxyDetail.Password,
					ProxyType = (int) addInstagram.ProxyDetail.proxyType
				};

				var result = await ProxyLogic.TestConnectivity(new Proxy.Models.ProxyModelShort
				{
					HostAddress = useProxyFromUser.HostAddress,
					NeedServerAuth = useProxyFromUser.NeedServerAuth,
					Password = useProxyFromUser.Password,
					Port = useProxyFromUser.Port,
					ProxyType = (ProxyType) useProxyFromUser.ProxyType,
					Username = useProxyFromUser.Username
				});

				if (string.IsNullOrEmpty(result))
				{
					resultCarrier.IsSuccessful = false;
					resultCarrier.Info = new ErrorResponse
					{
						Message = "Failed to connect to proxy"
					};
					return resultCarrier;
				}
			}

			try
			{
				var instagramAccountModel = new InstagramAccountModel
				{
					AccountId = accountId,
					DeviceDetail = AndroidDeviceGenerator.GetRandomAndroidDevice(),
					FollowersCount = null,
					FollowingCount = null,
					Password = addInstagram.Password,
					Username = addInstagram.Username,
					TotalPostsCount = null,
					Type = addInstagram.Type,
					AgentState = (int)AgentState.NotStarted,
					LastPurgeCycle = null,
					DateAdded = DateTime.UtcNow,
					SleepTimeRemaining = null,
					Email = null,
					IsBusiness = null,
					PhoneNumber = null,
					UserBiography = null,
					UserLimits = null,
					Location = null,
					ChallengeInfo = null,
					UserId = null,
					BlockedActions = new List<BlockedAction>()
				};

				var result = await _instagramAccountRepository.AddInstagramAccount(instagramAccountModel);

				if (result != null)
				{
					await _publisher.PublishAsync(new InstagramAccountPublishEventModel
					{
						InstagramAccount = new Events.Models.PublishObjects.InstagramAccountModel
						{
							_id = result._id,
							AccountId = result.AccountId,
							AgentState = result.AgentState,
							Email = result.Email,
							EmailPassword = result.EmailPassword,
							FollowersCount = result.FollowersCount,
							FollowingCount = result.FollowingCount,
							FullName = result.FullName,
							Location = result.Location,
							Password = result.Password,
							TotalPostsCount = result.TotalPostsCount,
							Type = result.Type,
							UserId = result.UserId,
							Username = result.Username,
							ProfilePicture = result.ProfilePicture,
							PhoneNumber = result.PhoneNumber
						},
						IpAddress = addInstagram.IpAddress,
						Location = addInstagram.Location,
						UserProxy = useProxyFromUser
					});

					resultCarrier.IsSuccessful = true;
					resultCarrier.Results = new AddInstagramAccountResponse
					{
						AccountId = result.AccountId,
						InstagramAccountId = result._id,
					};

					return resultCarrier;
				}
				resultCarrier.Info = new ErrorResponse { Message = $"Failed to add instagram user of {result.AccountId}, instagram account {result.Username}" };
				return resultCarrier;
			}
			catch (Exception ee)
			{
				resultCarrier.Info = new ErrorResponse
				{
					Message = $"Exception trying to add user: {addInstagram.Username}, error: {ee.Message}",
					Exception = ee
				};
				await _reportHandler.MakeReport(ee);
				return resultCarrier;
			}
		}
		public async Task<StateData> GetInstagramAccountStateData(string accountId, string instagramAccountId)
		{
			try
			{
				var state = await _instagramAccountRepository.GetInstagramAccountStateData(accountId, instagramAccountId);
				return state.Results ?? null;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<ShortInstagramAccountModel> GetInstagramAccountShort(string accountId, string instagramAccountId)
		{
			try
			{
				//var redisRes = await _instagramAccountRedis.GetInstagramAccountDetail(accountId,instagramAccountId);
				//if (redisRes != null && redisRes.Recreate().Count>3 && redisRes.Id!=null)
				//{
				//	return redisRes;
				//}
				var account = await _instagramAccountRepository.GetInstagramAccount(accountId, instagramAccountId);
				if (account.Results == null) return null;
				var res = account.Results;
				var shortInst = new ShortInstagramAccountModel
				{
					AccountId = res.AccountId,
					AgentState = res.AgentState,
					LastPurgeCycle = res.LastPurgeCycle,
					FollowersCount = res.FollowersCount,
					FollowingCount = res.FollowingCount,
					Id = res._id,
					TotalPostsCount = res.TotalPostsCount,
					Username = res.Username,
					DateAdded = res.DateAdded,
					SleepTimeRemaining = res.SleepTimeRemaining,
					Email = res.Email,
					PhoneNumber = res.PhoneNumber,
					FullName = res.FullName,
					ProfilePicture = res.ProfilePicture,
					UserBiography = res.UserBiography,
					UserLimits = res.UserLimits,
					IsBusiness = res.IsBusiness,
					Location = res.Location,
					Type = res.Type,
					ChallengeInfo = res.ChallengeInfo,
					UserId = res.UserId,
					BlockedActions = res.BlockedActions,
				};
				//await _instagramAccountRedis.SetInstagramAccountDetail(accountId, instagramAccountId, shortInst);
				return shortInst;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<InstagramAccountModel> GetInstagramAccount(string accountId, string instagramAccountId)
		{
			try
			{
				var account = await _instagramAccountRepository.GetInstagramAccount(accountId, instagramAccountId);

				return account.Results;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsFull(int type)
		{
			try
			{
				var account = await _instagramAccountRepository.GetInstagramAccountsFull(type);
				return account;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountsOfUser(string accountId, int type = 0)
		{
			try
			{
				//var redisGet = await _instagramAccountRedis.GetWorkerAccounts();
				//if (redisGet.Any())
				//{
				//	return redisGet;
				//}
				var account = await _instagramAccountRepository.GetInstagramAccountsOfUser(accountId, type);
				return account.Results?.Select(res => new ShortInstagramAccountModel
				{
					AccountId = res.AccountId,
					AgentState = res.AgentState,
					LastPurgeCycle = res.LastPurgeCycle,
					FollowersCount = res.FollowersCount,
					FollowingCount = res.FollowingCount,
					Id = res._id,
					TotalPostsCount = res.TotalPostsCount,
					Username = res.Username,
					DateAdded = res.DateAdded,
					SleepTimeRemaining = res.SleepTimeRemaining,
					Email = res.Email,
					PhoneNumber = res.PhoneNumber,
					FullName = res.FullName,
					ProfilePicture = res.ProfilePicture,
					UserBiography = res.UserBiography,
					UserLimits = res.UserLimits,
					IsBusiness = res.IsBusiness,
					Location = res.Location,
					ChallengeInfo = res.ChallengeInfo,
					UserId = res.UserId,
					Type = res.Type,
					BlockedActions = res.BlockedActions
				});
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<bool> ClearCacheData(string accountId, string instagramAccountId)
		{
			var account = await GetInstagramAccount(accountId, instagramAccountId);
			if (account == null) return false;
			
			var androidDevice = account.State?.DeviceInfo;
			if (androidDevice == null) return true;

			var newState = new StateData
			{
				DeviceInfo = androidDevice,
				UserSession = account.State.UserSession,
				IsAuthenticated = account.State.IsAuthenticated,
				InstaApiVersion = account.State.InstaApiVersion,
				Cookies = new CookieContainer(),
				RawCookies = new List<Cookie>()
			};

			return await _instagramAccountRepository.ClearCacheData(accountId, instagramAccountId, newState);
		}
		public async Task<long?> PartialUpdateInstagramAccount(string accountId, string instagramAccountId, InstagramAccountModel instagramAccountModel)
		{
			#region Redis
			//var toshortmodel = new ShortInstagramAccountModel
			//{
			//	AccountId = instagramAccountModel.AccountId,
			//	AgentState = instagramAccountModel.AgentState,
			//	LastPurgeCycle = instagramAccountModel.LastPurgeCycle,
			//	FollowersCount = instagramAccountModel.FollowersCount,
			//	FollowingCount = instagramAccountModel.FollowingCount,
			//	Id = instagramAccountModel._id,
			//	TotalPostsCount = instagramAccountModel.TotalPostsCount,
			//	Username = instagramAccountModel.Username,
			//	DateAdded = instagramAccountModel.DateAdded,
			//	SleepTimeRemaining = instagramAccountModel.SleepTimeRemaining,
			//	Email = instagramAccountModel.Email,
			//	PhoneNumber = instagramAccountModel.PhoneNumber,
			//	FullName = instagramAccountModel.FullName,
			//	ProfilePicture = instagramAccountModel.ProfilePicture,
			//	UserBiography = instagramAccountModel.UserBiography,
			//	UserLimits = instagramAccountModel.UserLimits,
			//	IsBusiness = instagramAccountModel.IsBusiness,
			//	Location = instagramAccountModel.Location,
			//	Type = instagramAccountModel.Type,
			//	ChallengeInfo = instagramAccountModel.ChallengeInfo
			//};
			//if(!(await _instagramAccountRedis.AccountExists(accountId, instagramAccountId)))
			//{
			//	var lastUpdatedDetails = (await _instagramAccountRepository.GetInstagramAccount(accountId, instagramAccountId)).Results;
			//	toshortmodel = toshortmodel.CreateNewObjectIgnoringNulls(new ShortInstagramAccountModel
			//	{
			//		AccountId = lastUpdatedDetails.AccountId,
			//		SleepTimeRemaining = lastUpdatedDetails.SleepTimeRemaining,
			//		AgentState = lastUpdatedDetails.AgentState,
			//		DateAdded = lastUpdatedDetails.DateAdded,
			//		Email = lastUpdatedDetails.Email,
			//		FollowersCount = lastUpdatedDetails.FollowersCount,
			//		FollowingCount = lastUpdatedDetails.FollowingCount,
			//		FullName = lastUpdatedDetails.FullName,
			//		Id = lastUpdatedDetails._id,
			//		IsBusiness = lastUpdatedDetails.IsBusiness,
			//		LastPurgeCycle = lastUpdatedDetails.LastPurgeCycle,
			//		Location = lastUpdatedDetails.Location,
			//		PhoneNumber = lastUpdatedDetails.PhoneNumber,
			//		ProfilePicture = lastUpdatedDetails.ProfilePicture,
			//		TotalPostsCount = lastUpdatedDetails.TotalPostsCount,
			//		UserBiography = lastUpdatedDetails.UserBiography,
			//		UserLimits = lastUpdatedDetails.UserLimits,
			//		Username = lastUpdatedDetails.Username,
			//		ChallengeInfo = lastUpdatedDetails.ChallengeInfo
			//	});
			//}
			//await _instagramAccountRedis.SetInstagramAccountDetail(accountId,instagramAccountId,toshortmodel);
			#endregion
			return await _instagramAccountRepository.PartialUpdateInstagramAccount(instagramAccountId, instagramAccountModel);
		}

		public async Task<bool> AddBlockedAction(string instagramAccountId, ActionType actionType, DateTime? blockFor = null)
			=> await _instagramAccountRepository.AddBlockedAction(instagramAccountId, actionType, blockFor);
		
		public async Task<bool> RemoveBlockedAction(string instagramAccountId, ActionType actionType)
			=> await _instagramAccountRepository.RemoveBlockedAction(instagramAccountId, actionType);
		
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1)
		{
			try
			{
				//var redist = await _instagramAccountRedis.GetInstagramAccountActiveDetail();
				//var activeAgentInstagramAccounts = redist as ShortInstagramAccountModel[] ?? redist.ToArray();
				//if (activeAgentInstagramAccounts.Any())
				//{
				//	return activeAgentInstagramAccounts;
				//}
				var account = await _instagramAccountRepository.GetActiveAgentInstagramAccounts(actionExType);
				return account;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type)
		{
			try
			{
				var account = await _instagramAccountRepository.GetInstagramAccounts(type);
				return account;
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task EmptyChallengeInfo(string instagramAccountId)
		{
			try
			{
				await _instagramAccountRepository.EmptyChallengeInfo(instagramAccountId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
			}
		}

		public async Task<bool> RemoveInstagramAccount(string instagramAccountId)
		{
			var results = await _instagramAccountRepository.RemoveInstagramAccount(instagramAccountId);
			if (!results) return false;
			await _publisher.PublishAsync(new InstagramAccountDeletePublishEvent
			{
				InstagramAccountId = instagramAccountId
			});
			return true;
		}

		public async Task UpdateAgentStates(AgentState state, int accountType = 1, AgentState targetedStates = AgentState.Working)
		{
			if (accountType == 0)
				return;
			await _instagramAccountRepository.UpdateMultipleUserAgentStates(state, accountType, targetedStates);
		}
	}
}

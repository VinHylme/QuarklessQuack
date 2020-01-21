using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Events.Interfaces;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Logic.InstagramAccounts
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

		public async Task<ResultCarrier<AddInstagramAccountResponse>> AddInstagramAccount(string accountId, StateData state, AddInstagramAccountRequest addInstagram)
		{
			var resultCarrier = new ResultCarrier<AddInstagramAccountResponse>();
			try
			{
				var device = state.DeviceInfo.DeviceModel;

				var instagramAccountModel = new InstagramAccountModel
				{
					Device = device,
					State = state,
					AccountId = accountId,
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
					FullName = state.UserSession.LoggedInUser.FullName,
					IsBusiness = null,
					PhoneNumber = null,
					ProfilePicture = state.UserSession.LoggedInUser.ProfilePicture ?? state.UserSession.LoggedInUser.ProfilePicUrl,
					UserBiography = null,
					UserLimits = null,
					Location = null,
					ChallengeInfo = null,
					UserId = null
				};

				var result = await _instagramAccountRepository.AddInstagramAccount(instagramAccountModel);

				if (result != null)
				{
					await _publisher.PublishAsync(new InstagramAccountPublishEventModel
					{
						InstagramAccount = result,
						IpAddress = addInstagram.ComingFrom
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
					UserId = res.UserId
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
					Type = res.Type
				});
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
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
	}
}

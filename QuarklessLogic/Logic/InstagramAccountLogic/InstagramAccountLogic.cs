using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessRepositories.InstagramAccountRepository;
using QuarklessRepositories.RedisRepository.InstagramAccountRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessLogic.Handlers.ReportHandler;

namespace QuarklessLogic.Logic.InstagramAccountLogic
{
	public class InstagramAccountLogic : IInstagramAccountLogic
	{
		private readonly IProxyLogic _proxyLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IInstagramAccountRepository _instagramAccountRepository;
		private readonly IInstagramAccountRedis _instagramAccountRedis;
		private readonly IReportHandler _reportHandler;
		public InstagramAccountLogic(IProxyLogic proxyLogic, IInstagramAccountRepository instagramAccountRepository, 
			IInstagramAccountRedis instagramAccountRedis, IProfileLogic profileLogic, IReportHandler reportHandler)
		{
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_instagramAccountRepository = instagramAccountRepository;
			_instagramAccountRedis = instagramAccountRedis;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("InstagramAccountLogic");
		}
		public async Task<ResultCarrier<AddInstagramAccountResponse>> AddInstagramAccount(string accountId, StateData state, AddInstagramAccountRequest addInstagram)
		{
			var @Result = new ResultCarrier<AddInstagramAccountResponse>();
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
					TotalPostsCount = null,
					Type = addInstagram.Type,
					AgentState = (int) AgentState.NotStarted,
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
					ChallengeInfo = null
				};
				var result = await _instagramAccountRepository.AddInstagramAccount(instamodel);
				if(result!=null)
				{
					var createEmptyProfile = await _profileLogic.AddProfile(new QuarklessContexts.Models.Profiles.ProfileModel
					{
						Account_Id = result.AccountId,
						InstagramAccountId = result._id,
						Topics = new QuarklessContexts.Models.Profiles.Topics
						{
							SubTopics = new List<QuarklessContexts.Models.Profiles.SubTopics>(),
						},
						Description = "Add a description about this profile",
						Name = "My Profile 1",
						AdditionalConfigurations = new QuarklessContexts.Models.Profiles.AdditionalConfigurations
						{
							ImageType = 0,
							IsTumblry = false,
							SearchTypes = new List<int> { 0, 1, 2 }
						},
						AutoGenerateTopics = false,
						Language = "English",
						Theme = new QuarklessContexts.Models.Profiles.Themes
						{
							Name = "My Cool Theme",
							Percentage = 20,
							Colors = new List<QuarklessContexts.Models.Profiles.Color>(),
							ImagesLike = new List<QuarklessContexts.Models.Profiles.GroupImagesAlike>()
						},
						LocationTargetList = new List<QuarklessContexts.Models.Profiles.Location>(),
						UserTargetList = new List<string>(),
						UserLocation = new QuarklessContexts.Models.Profiles.Location()
					});
					if (createEmptyProfile != null) { 
						var assign = await _proxyLogic.AssignProxy(new AssignedTo { Account_Id = result.AccountId, InstaId = result._id.ToString()});
						if (assign)
						{
							Result.IsSuccesful = true;
							Result.Results = new AddInstagramAccountResponse{
								AccountId = result.AccountId,
								InstagramAccountId = result._id,
								ProfileId = createEmptyProfile._id
							};
							return Result;
						}
						Result.Info = new ErrorResponse
						{
							Message =  $"Failed to assign proxy to user {result.AccountId}, instagram account {result.Username}",
						};
					}
				}
				Result.Info = new ErrorResponse{Message = $"Failed to add instagram user of {result.AccountId}, instagram account {result.Username}" };
				return Result;
			}
			catch(Exception ee)
			{
				Result.Info = new ErrorResponse
				{ 
					Message = $"Exepction trying to add user: {addInstagram.Username}, error: {ee.Message}",
					Exception = ee
				};
				_reportHandler.MakeReport(ee);
				return Result;
			}	
		}
		public async Task<StateData> GetInstagramAccountStateData(string accountId, string InstagramAccountId)
		{
			try
			{
				var state  = await _instagramAccountRepository.GetInstagramAccountStateData(accountId, InstagramAccountId);
				return state.Results ?? null;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
				var shortInst = new ShortInstagramAccountModel{
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
					ChallengeInfo = res.ChallengeInfo
				};
				//await _instagramAccountRedis.SetInstagramAccountDetail(accountId, instagramAccountId, shortInst);
				return shortInst;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
				return account.Results?.Select(res=>new ShortInstagramAccountModel
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
					ChallengeInfo = res.ChallengeInfo
				});
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts()
		{
			try
			{
				//var redist = await _instagramAccountRedis.GetInstagramAccountActiveDetail();
				//var activeAgentInstagramAccounts = redist as ShortInstagramAccountModel[] ?? redist.ToArray();
				//if (activeAgentInstagramAccounts.Any())
				//{
				//	return activeAgentInstagramAccounts;
				//}
				var account = await _instagramAccountRepository.GetActiveAgentInstagramAccounts();
				return account;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
			}
		}
	}
}

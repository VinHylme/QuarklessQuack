using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.InstaUserLogic
{
	public class InstaUserLogic : IInstaUserLogic
	{
		private readonly IReportHandler _reportHandler;
		private readonly IAPIClientContainer Client;

		public InstaUserLogic(IAPIClientContainer clientContexter, IReportHandler reportHandler)
		{
			Client = clientContexter;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Logic/InstaUser");
		}

		public async Task<IResult<InstaLoginResult>> TryLogin(string username, string password)
		{
			try
			{
				return await Client.EmptyClient.TryLogin(username, password);
				
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> AcceptConsent()
		{
			try
			{
				return await Client.GetContext.ActionClient.AcceptConsentAsync();
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<string> GetStateDataFromString()
		{
			try
			{
				return await Client.EmptyClient.GetStateDataFromString();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipStatus>> AcceptFriendshipRequestAsync(long userId)
		{
			try
			{
				return await Client.User.AcceptFriendshipRequestAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaChallengeRequireEmailVerify>> RequestVerifyCodeToEmailForChallengeRequireAsync(string username, string password)
		{
			try
			{
				return await Client.EmptyClient.ReturnClient.RequestVerifyCodeToEmailForChallengeRequireAsync();
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaChallengeRequireSMSVerify>> RequestVerifyCodeToSMSForChallengeRequireAsync(string username, string password)
		{
			try
			{
				return await Client.EmptyClient.ReturnClient.RequestVerifyCodeToSMSForChallengeRequireAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public InstaChallengeLoginInfo GetChallangeInfo()
		{
			try
			{
				return Client.EmptyClient.ReturnClient.GetChallengeLoginInfo;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password)
		{
			try
			{
				return await Client.EmptyClient.GetChallengeRequireVerifyMethodAsync(username,password);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaLoginResult>> SubmitChallangeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			try
			{
				return await Client.EmptyClient.SubmitChallangeCode(username, password, instaChallengeLoginInfo, code);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> BlockUser(long userId)
		{
			try
			{
				return await Client.User.BlockUserAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> FavoriteUser(long userId)
		{
			try
			{
				return await Client.User.FavoriteUserAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> FavoriteUserStories(long userId)
		{
			try
			{
				return await Client.User.FavoriteUserStoriesAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> FollowUser(long userId)
		{
			try
			{
				return await Client.User.FollowUserAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetCurrentUserFollowers(int limit)
		{
			try
			{
				return await Client.User.GetCurrentUserFollowersAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaActivityFeed>> GetFollowingActivityFeed(int limit)
		{
			try
			{
				return await Client.User.GetFollowingRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipShortStatusList>> GetFriendshipStatuses(params long[] userIds)
		{
			try
			{
				return await Client.User.GetFriendshipStatusesAsync(userIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFullUserInfo>> GetFullUserInfo(long userId)
		{
			try
			{
				return await Client.User.GetFullUserInfoAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaPendingRequest>> GetPendingFriendRequest()
		{
			try
			{
				return await Client.User.GetPendingFriendRequestsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaActivityFeed>> GetRecentActivityFeedAsync(int limit)
		{
			try
			{
				return await Client.User.GetRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaSuggestionItemList>> GetSuggestionDetails(long userId,long[] chainedUserIds = null)
		{
			try
			{
				return await Client.User.GetSuggestionDetailsAsync(userId,chainedUserIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaSuggestions>> GetUserSuggestions(int limit)
		{
			try
			{
				return await Client.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUser>> GetUser(string username)
		{
			try
			{
				return await Client.User.GetUserAsync(username);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetUserFollowers(string username, int limit, string query = "", bool mutalfirst = false)
		{
			try
			{
				return await Client.User.GetUserFollowersAsync(username, PaginationParameters.MaxPagesToLoad(limit), query, mutalfirst);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetUserFollowing(string username, int limit, string query = "")
		{
			try
			{
				return await Client.User.GetUserFollowingAsync(username, PaginationParameters.MaxPagesToLoad(limit), query);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUser>> GetUserNametag(InstaImage nametagmage)
		{
			try
			{
				return await Client.User.GetUserFromNametagAsync(nametagmage);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUserInfo>> GetUserInfo(long userpk)
		{
			try
			{
				return await Client.User.GetUserInfoByIdAsync(userpk);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaUserInfo>> GetUserInfoUsername(string username)
		{
			try
			{
				return await Client.User.GetUserInfoByUsernameAsync(username);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserMedia(string username, int limit)
		{
			try
			{
				return await Client.User.GetUserMediaAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserShoppableMedia(string username, int limit)
		{
			try
			{
				return await Client.User.GetUserShoppableMediaAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserTags(string username, int limit)
		{
			try
			{
				return await Client.User.GetUserTagsAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> HideStoryFromUser(long userid)
		{
			try
			{
				return await Client.User.HideMyStoryFromUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> IgnoreFriendship(long userid)
		{
			try
			{
				return await Client.User.IgnoreFriendshipRequestAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> MarkUserAsOverage(long userid)
		{
			try
			{
				return await Client.User.MarkUserAsOverageAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> MuteFriendStory(long userid)
		{
			try
			{
				return await Client.User.MuteFriendStoryAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> MuteUserMedia(long userid, InstaMuteOption muteOption)
		{
			try
			{
				return await Client.User.MuteUserMediaAsync(userid, muteOption);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipStatus>> RemoveFollower(long userid)
		{
			try
			{
				return await Client.User.RemoveFollowerAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> ReportUser(long userid)
		{
			try
			{
				return await Client.User.ReportUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<string>> TranslateBio(long userid)
		{
			try
			{
				return await Client.User.TranslateBiographyAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> UnBlockUser(long userid)
		{
			try
			{
				return await Client.User.UnBlockUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> UnFavoriteUser(long userid)
		{
			try
			{
				return await Client.User.UnFavoriteUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> UnFavoriteUserStories(long userid)
		{
			try
			{
				return await Client.User.UnFavoriteUserStoriesAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> UnFollowUser(long userid)
		{
			try
			{
				return await Client.User.UnFollowUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnHideMyStoryFromUser(long userid)
		{
			try
			{
				return await Client.User.UnHideMyStoryFromUserAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnMuteFriendStory(long userid)
		{
			try
			{
				return await Client.User.UnMuteFriendStoryAsync(userid);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnMuteUserMedia(long userid, InstaMuteOption muteOption)
		{
			try
			{
				return await Client.User.UnMuteUserMediaAsync(userid, muteOption);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
	}
}

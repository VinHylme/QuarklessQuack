using System;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Base.InstagramUser
{
	public class InstaUserLogic : IInstaUserLogic
	{
		private readonly IReportHandler _reportHandler;
		private readonly IApiClientContainer _clientContainer;
		public InstaUserLogic(IApiClientContainer clientContainer, IReportHandler reportHandler)
		{
			_clientContainer = clientContainer;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Logic/InstaUser");
		}

		public async Task<IResult<InstaFriendshipStatus>> AcceptFriendshipRequestAsync(long userId)
		{
			try
			{
				return await _clientContainer.User.AcceptFriendshipRequestAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<bool> SubmitPhoneVerify(string phoneNumber, InstaChallengeLoginInfo challengeLoginInfo)
		{
			try
			{
				if (_clientContainer.GetContext.Container.ActionClient.ChallengeLoginInfo == null)
					_clientContainer.GetContext.Container.ActionClient.ChallengeLoginInfo = challengeLoginInfo;
				
				var results = await _clientContainer.GetContext.Container.ActionClient
					.SubmitPhoneNumberForChallengeRequireAsync(phoneNumber);

				return results.Succeeded;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return false;
			}
		}
		public async Task<SubmitChallengeResponse> SubmitChallengeCode(string username, string password,
			InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			try
			{
				if (_clientContainer.GetContext.Container.ActionClient.ChallengeLoginInfo == null)
					_clientContainer.GetContext.Container.ActionClient.ChallengeLoginInfo = instaChallengeLoginInfo;

				var results = await _clientContainer.GetContext.Container
					.ActionClient.VerifyCodeForChallengeRequireAsync(code);

				return new SubmitChallengeResponse
				{
					Result = results,
					InstagramId = _clientContainer?.GetContext?.Container?.InstagramAccount?.Id
				};
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		
		public async Task<IResult<InstaFriendshipFullStatus>> BlockUser(long userId)
		{
			try
			{
				return await _clientContainer.User.BlockUserAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> FavoriteUser(long userId)
		{
			try
			{
				return await _clientContainer.User.FavoriteUserAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> FavoriteUserStories(long userId)
		{
			try
			{
				return await _clientContainer.User.FavoriteUserStoriesAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> FollowUser(long userId)
		{
			try
			{
				return await _clientContainer.User.FollowUserAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetCurrentUserFollowers(int limit)
		{
			try
			{
				return await _clientContainer.User.GetCurrentUserFollowersAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaActivityFeed>> GetFollowingActivityFeed(int limit)
		{
			try
			{
				return await _clientContainer.User.GetFollowingRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipShortStatusList>> GetFriendshipStatuses(params long[] userIds)
		{
			try
			{
				return await _clientContainer.User.GetFriendshipStatusesAsync(userIds);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFullUserInfo>> GetFullUserInfo(long userId)
		{
			try
			{
				return await _clientContainer.User.GetFullUserInfoAsync(userId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaPendingRequest>> GetPendingFriendRequest()
		{
			try
			{
				return await _clientContainer.User.GetPendingFriendRequestsAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaActivityFeed>> GetRecentActivityFeedAsync(int limit)
		{
			try
			{
				return await _clientContainer.User.GetRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaSuggestionItemList>> GetSuggestionDetails(long userId,long[] chainedUserIds = null)
		{
			try
			{
				return await _clientContainer.User.GetSuggestionDetailsAsync(userId,chainedUserIds);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaSuggestions>> GetUserSuggestions(int limit)
		{
			try
			{
				return await _clientContainer.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUser>> GetUser(string username)
		{
			try
			{
				return await _clientContainer.User.GetUserAsync(username);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetUserFollowers(string username, int limit, string query = "", bool mutalfirst = false)
		{
			try
			{
				return await _clientContainer.User.GetUserFollowersAsync(username, PaginationParameters.MaxPagesToLoad(limit), query, mutalfirst);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserShortList>> GetUserFollowing(string username, int limit, string query = "")
		{
			try
			{
				return await _clientContainer.User.GetUserFollowingAsync(username, PaginationParameters.MaxPagesToLoad(limit), query);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUser>> GetUserNametag(InstaImage nametagmage)
		{
			try
			{
				return await _clientContainer.User.GetUserFromNametagAsync(nametagmage);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserInfo>> GetUserInfo(long userpk)
		{
			try
			{
				return await _clientContainer.User.GetUserInfoByIdAsync(userpk);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserInfo>> GetUserInfoUsername(string username)
		{
			try
			{
				return await _clientContainer.User.GetUserInfoByUsernameAsync(username);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserMedia(string username, int limit)
		{
			try
			{
				return await _clientContainer.User.GetUserMediaAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserShoppableMedia(string username, int limit)
		{
			try
			{
				return await _clientContainer.User.GetUserShoppableMediaAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetUserTags(string username, int limit)
		{
			try
			{
				return await _clientContainer.User.GetUserTagsAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> HideStoryFromUser(long userid)
		{
			try
			{
				return await _clientContainer.User.HideMyStoryFromUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> IgnoreFriendship(long userid)
		{
			try
			{
				return await _clientContainer.User.IgnoreFriendshipRequestAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> MarkUserAsOverage(long userid)
		{
			try
			{
				return await _clientContainer.User.MarkUserAsOverageAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> MuteFriendStory(long userid)
		{
			try
			{
				return await _clientContainer.User.MuteFriendStoryAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> MuteUserMedia(long userid, InstaMuteOption muteOption)
		{
			try
			{
				return await _clientContainer.User.MuteUserMediaAsync(userid, muteOption);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipStatus>> RemoveFollower(long userid)
		{
			try
			{
				return await _clientContainer.User.RemoveFollowerAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ReportUser(long userid)
		{
			try
			{
				return await _clientContainer.User.ReportUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<string>> TranslateBio(long userid)
		{
			try
			{
				return await _clientContainer.User.TranslateBiographyAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> UnBlockUser(long userid)
		{
			try
			{
				return await _clientContainer.User.UnBlockUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnFavoriteUser(long userid)
		{
			try
			{
				return await _clientContainer.User.UnFavoriteUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnFavoriteUserStories(long userid)
		{
			try
			{
				return await _clientContainer.User.UnFavoriteUserStoriesAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFriendshipFullStatus>> UnFollowUser(long userid)
		{
			try
			{
				return await _clientContainer.User.UnFollowUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnHideMyStoryFromUser(long userid)
		{
			try
			{
				return await _clientContainer.User.UnHideMyStoryFromUserAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnMuteFriendStory(long userid)
		{
			try
			{
				return await _clientContainer.User.UnMuteFriendStoryAsync(userid);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStoryFriendshipStatus>> UnMuteUserMedia(long userid, InstaMuteOption muteOption)
		{
			try
			{
				return await _clientContainer.User.UnMuteUserMediaAsync(userid, muteOption);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}

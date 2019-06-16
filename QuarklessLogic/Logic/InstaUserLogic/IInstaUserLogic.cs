using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;

namespace QuarklessLogic.Logic.InstaUserLogic
{
	public interface IInstaUserLogic
	{
		Task<string> GetStateDataFromString();
		Task<bool> TryLogin(string username, string password);

		Task<IResult<InstaActivityFeed>> GetRecentActivityFeedAsync(int limit);
		Task<IResult<InstaFriendshipStatus>> AcceptFriendshipRequestAsync(long userId);
		Task<IResult<InstaFriendshipFullStatus>> BlockUser(long userId);
		Task<IResult<bool>> FavoriteUser(long userId);
		Task<IResult<bool>> FavoriteUserStories(long userId);
		Task<IResult<InstaFriendshipFullStatus>> FollowUser(long userId);
		Task<IResult<InstaUserShortList>> GetCurrentUserFollowers(int limit);
		Task<IResult<InstaActivityFeed>> GetFollowingActivityFeed(int limit);
		Task<IResult<InstaFriendshipShortStatusList>> GetFriendshipStatuses(params long[] userIds);
		Task<IResult<InstaFullUserInfo>> GetFullUserInfo(long userId);
		Task<IResult<InstaPendingRequest>> GetPendingFriendRequest();
		Task<IResult<InstaSuggestionItemList>> GetSuggestionDetails(params long[] userIds);
		Task<IResult<InstaUser>> GetUser(string username);
		Task<IResult<InstaUserShortList>> GetUserFollowers(string username, int limit, string query = "", bool mutalfirst = false);
		Task<IResult<InstaUserShortList>> GetUserFollowing(string username, int limit, string query = "");
		Task<IResult<InstaUserInfo>> GetUserInfo(long userpk);
		Task<IResult<InstaUserInfo>> GetUserInfoUsername(string username);
		Task<IResult<InstaMediaList>> GetUserMedia(string username, int limit);
		Task<IResult<InstaUser>> GetUserNametag(InstaImage nametagmage);
		Task<IResult<InstaMediaList>> GetUserTags(string username, int limit);
		Task<IResult<InstaMediaList>> GetUserShoppableMedia(string username, int limit);
		Task<IResult<InstaSuggestions>> GetUserSuggestions(int limit);
		Task<IResult<InstaStoryFriendshipStatus>> HideStoryFromUser(long userid);
		Task<IResult<InstaFriendshipFullStatus>> IgnoreFriendship(long userid);
		Task<IResult<bool>> MarkUserAsOverage(long userid);
		Task<IResult<InstaStoryFriendshipStatus>> MuteFriendStory(long userid);
		Task<IResult<InstaStoryFriendshipStatus>> MuteUserMedia(long userid, InstaMuteOption muteOption);
		Task<IResult<InstaFriendshipStatus>> RemoveFollower(long userid);
		Task<IResult<bool>> ReportUser(long userid);
		Task<IResult<string>> TranslateBio(long userid);
		Task<IResult<InstaFriendshipFullStatus>> UnBlockUser(long userid);
		Task<IResult<bool>> UnFavoriteUser(long userid);
		Task<IResult<bool>> UnFavoriteUserStories(long userid);
		Task<IResult<InstaFriendshipFullStatus>> UnFollowUser(long userid);
		Task<IResult<InstaStoryFriendshipStatus>> UnHideMyStoryFromUser(long userid);
		Task<IResult<InstaStoryFriendshipStatus>> UnMuteFriendStory(long userid);
		Task<IResult<InstaStoryFriendshipStatus>> UnMuteUserMedia(long userid, InstaMuteOption muteOption);
	}
}
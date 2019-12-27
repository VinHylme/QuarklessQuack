using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.ContentSearch.InstagramSearch
{
	public interface IInstagramContentSearch
	{
		Task<IEnumerable<UserResponse<string>>> GetUsersFollowersList(string username, int limit,
			string query = null, bool mutualFirst = true);
		Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit);
		Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null);
		Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId);
		Task<List<UserResponse<InstaComment>>> SearchInstagramMediaCommenters(CTopic mediaTopic, string mediaId, int limit);
		Task<List<UserResponse<string>>> SearchInstagramMediaLikers(CTopic mediaTopic, string mediaId);
		Task<Media> SearchRecentLocationMediaDetailInstagram(Location location, int limit);
		Task<Media> SearchTopLocationMediaDetailInstagram(Location location, int limit);
		Task<Media> SearchMediaDetailInstagram(IEnumerable<string> topics, int limit, bool isRecent = false);
		Task<Media> SearchMediaDetailInstagram(IEnumerable<CTopic> topics, int limit, bool isRecent = false);
		Task<Media> SearchMediaInstagram(IEnumerable<CTopic> topics, InstaMediaType mediaType, int limit);
		Task<Media> SearchMediaUser(string username = null, int limit = 1);
		Task<Media> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1);
		Task<Media> SearchUsersMediaDetailInstagram(string userName, int limit);
		Task<InstaDirectInboxContainer> SearchUserInbox(int limit = 1);
	}
}
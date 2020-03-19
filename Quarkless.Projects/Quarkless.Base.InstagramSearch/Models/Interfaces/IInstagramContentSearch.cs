using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Topic;
using Quarkless.Models.SearchResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.InstagramSearch.Models.Interfaces
{
	public interface IInstagramContentSearch
	{
		Task<IEnumerable<UserResponse<InstaStory>>> GetUserStoriesByTopic(CTopic topic, int limit = 1);
		Task<IEnumerable<UserResponse<InstaReelFeed>>> GetUserFeedStories(int limit = 1);
		Task<IEnumerable<UserResponse<string>>> GetUsersFollowersList(string username, int limit,
			string query = null, bool mutualFirst = true);
		Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit);
		Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null);
		Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId);
		Task<List<UserResponse<InstaComment>>> SearchInstagramMediaCommenters(CTopic mediaTopic, string mediaId, int limit);
		Task<List<UserResponse<string>>> SearchInstagramMediaLikers(CTopic mediaTopic, string mediaId);
		Task<Quarkless.Models.SearchResponse.Media> SearchRecentLocationMediaDetailInstagram(Location location, int limit);
		Task<Quarkless.Models.SearchResponse.Media> SearchTopLocationMediaDetailInstagram(Location location, int limit);
		Task<Quarkless.Models.SearchResponse.Media> SearchMediaDetailInstagram(IEnumerable<string> topics, int limit, bool isRecent = false);
		Task<Quarkless.Models.SearchResponse.Media> SearchMediaDetailInstagram(IEnumerable<CTopic> topics, int limit, bool isRecent = false);
		Task<Quarkless.Models.SearchResponse.Media> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1);
		Task<Quarkless.Models.SearchResponse.Media> SearchUsersMediaDetailInstagram(CTopic topic, string userName, int limit);
		Task<InstaDirectInboxContainer> SearchUserInbox(int limit = 1);
	}
}

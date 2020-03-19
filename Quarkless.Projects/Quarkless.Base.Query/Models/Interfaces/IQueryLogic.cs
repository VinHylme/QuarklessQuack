using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.ContentSearch.Models.Models;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Topic;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Query.Models.Interfaces
{
	public interface IQueryLogic
	{
		Task<IEnumerable<CommentMedia>> GetRecentComments(ProfileRequest profile);
		Task<ProfileConfiguration> GetProfileConfig();
		Task<IEnumerable<CTopic>> GetRelatedTopics(string topicParentId);
		object SearchPlaces(string query);
		object AutoCompleteSearchPlaces(string query, int radius = 500);
		Task<Quarkless.Models.SearchResponse.Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null, bool moreAccurate = false);
		Task<SubTopics> GetRelatedKeywords(string topicName);
		Task<IEnumerable<string>> BuildHashtags(SuggestHashtagRequest suggestHashtagRequest);

		Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByLocation(Location location,
			string username, string instagramAccountId, int limit = 1);

		Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByTopic(IEnumerable<string> topics,
			string username, string instagramAccountId, int limit = 1);
		Task<IEnumerable<MediaResponseSingle>> GetUsersMedia(ProfileRequest profile);
		Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetUsersFeed(ProfileRequest profile);
		Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetMediasTargetList(ProfileRequest profile);
		Task<IEnumerable<LookupContainer<UserResponse>>> GetUsersTargetList(ProfileRequest profile);
		Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetMediasByLocation(ProfileRequest profile);
		Task<IEnumerable<LookupContainer<UserResponse>>> GetUserByLocation(ProfileRequest profile);
		Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowingList(ProfileRequest profile);
		Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowerList(ProfileRequest profile);
		Task<Quarkless.Models.SearchResponse.Media> SearchMediasByTopic(IEnumerable<string> topics, string username, string instagramAccountId, int limit = 1);
		Task<Quarkless.Models.SearchResponse.Media> SearchMediasByLocation(Location location, string username, string instagramAccountId, int limit = 1);
		Task<IEnumerable<LookupContainer<UserResponse<UserSuggestionDetails>>>> GetUsersSuggestedFollowingList(ProfileRequest profile);
		Task<InstaDirectInbox> GetUserInbox(ProfileRequest profile);
	}
}
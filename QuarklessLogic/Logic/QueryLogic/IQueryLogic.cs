using Quarkless.Interfacing.Objects;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels.Settings;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.LookupModels;

namespace QuarklessLogic.Logic.QueryLogic
{
	public interface IQueryLogic
	{
		Task<ProfileConfiguration> GetProfileConfig();
		object SearchPlaces(string query);
		object AutoCompleteSearchPlaces(string query, int radius = 500);
		Task<Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null, bool moreAccurate = false);
		Task<SubTopics> GetRelatedKeywords(string topicName);
		Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language,
			int limit = 1, int pickRate = 20);

		Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByLocation(Location location,
			string username, string instagramAccountId, int limit = 1);

		Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByTopic(IEnumerable<string> topics,
			string username, string instagramAccountId, int limit = 1);
		Task<IEnumerable<MediaResponseSingle>> GetUsersMedia(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<Media>> GetUsersFeed(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<Media>> GetMediasTargetList(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<LookupContainer<UserResponse>>> GetUsersTargetList(SString accountId, SString instagramAccountId, SString topic);
		Task<IEnumerable<Media>> GetMediasByLocation(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<LookupContainer<UserResponse>>> GetUserByLocation(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowingList(SString username, SString instagramAccountId, SString topic);
		Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowerList(SString username, SString instagramAccountId, SString topic);
		Task<Media> SearchMediasByTopic(IEnumerable<string> topics, string username, string instagramAccountId, int limit = 1);
		Task<Media> SearchMediasByLocation(Location location, string username, string instagramAccountId, int limit = 1);
		Task<IEnumerable<LookupContainer<UserResponse<UserSuggestionDetails>>>> GetUsersSuggestedFollowingList(SString username, SString instagramAccountId, SString topic);
		Task<InstaDirectInbox> GetUserInbox(SString accountId, SString instagramAccountId, SString topic);
	}
}
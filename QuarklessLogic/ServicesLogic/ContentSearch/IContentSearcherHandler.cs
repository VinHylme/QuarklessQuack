using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Handlers.ClientProvider;

namespace QuarklessLogic.ServicesLogic.ContentSearch
{
	public interface IContentSearcherHandler
	{
		Task<IEnumerable<UserResponse<string>>> GetUsersFollowersList(string username, int limit,
			string query = null, bool mutualFirst = true);
		Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit);
		Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null);
		Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId);
		Task<List<UserResponse<InstaComment>>> SearchInstagramMediaCommenters(string mediaId, int limit);
		Task<List<UserResponse<string>>> SearchInstagramMediaLikers(string mediaId);
		Task<Media> SearchRecentLocationMediaDetailInstagram(Location location, int limit);
		Task<Media> SearchTopLocationMediaDetailInstagram(Location location, int limit);
		Task<Media> SearchMediaDetailInstagram(List<string> topics, int limit, bool isRecent = false);
		Task<Media> SearchMediaInstagram(List<string> topics, InstaMediaType mediaType, int limit);
		Task<Media> SearchMediaUser(string username = null, int limit = 1);
		Task<Media> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1);
		Task<Media> SearchUsersMediaDetailInstagram(string userName, int limit);
		SearchResponse<Media> SearchViaGoogle(SearchImageModel searchImageQuery);
		SearchResponse<Media> SearchViaYandex(YandexSearchQuery yandexSearchQuery, int limit);
		SearchResponse<Media> SearchYandexSimilarSafeMode(List<GroupImagesAlike> imagesAlikes, int limit);
		SearchResponse<Media> SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit, int offset = 0);
		SearchResponse<Media> SearchSimilarImagesViaGoogle(List<GroupImagesAlike> imagesAlikes, int limit, int offset = 0);
		Task<IEnumerable<TopicCategories>> GetBusinessCategories();
		void ChangeUser(IAPIClientContainer newUser);
	}
}
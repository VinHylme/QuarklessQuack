using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.ContentSearch
{
	public interface IContentSearch
	{
		Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit);
		Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null);
		ContextContainer SetUserClient(IUserStoreDetails _user);
		Task<IEnumerable<UserResponse<MediaDetail>>> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1);
		Task<IEnumerable<UserResponse<MediaDetail>>> SearchUsersMediaDetailInstagram(string userName, int limit);
		Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId);
		Task<List<UserResponse<string>>> SearchInstagramMediaLikers(string mediaId);
		Task<List<UserResponse<CommentResponse>>> SearchInstagramMediaCommenters(string mediaId, int limit);
		Task<IEnumerable<UserResponse<MediaDetail>>> SearchMediaDetailInstagram(List<string> topics, int limit);
		Media SearchViaGoogle(SearchImageModel searchImageQuery);
		Task<Media> SearchMediaInstagram(List<string> topics, InstaMediaType mediaType, int limit);
		Task<Media> SearchMediaUser(string username = null, int limit = 1);
		Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit);
	}
}
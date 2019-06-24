using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services.Interfaces
{
	public interface IContentManager
	{
		void SetUser(IUserStoreDetails user);
		string GenerateComment(TopicsModel topicsModel, string language);
		IEnumerable<UserResponse<MediaDetail>> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1);
		List<UserResponse<string>> SearchInstagramMediaLikers(string mediaId);
		List<UserResponse<CommentResponse>> SearchInstagramMediaCommenters(string mediaId, int limit);
		IEnumerable<UserResponse<MediaDetail>> SearchMediaDetailInstagram(List<string> topics, int limit);
		IEnumerable<UserResponse<MediaDetail>> SearchUsersMediaDetailInstagram(string userName, int limit);
		InstaFullUserInfo SearchInstagramFullUserDetail(long userId);
		string GenerateMediaInfo(TopicsModel topicSelect, string language);
		string GenerateText(string topic, string lang, int type, int limit, int size);
		Task<IEnumerable<string>> GetHashTags(string topic, int limit, int pickAmount);
		Task<List<TopicsModel>> GetTopics(List<string> topic, int limit);
		bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime);
		IEnumerable<PostsModel> GetUserMedia(string username = null, int limit = 1);
		IEnumerable<PostsModel> GetMediaInstagram(InstaMediaType mediaType, List<string> topics, int limit = 1);
		IEnumerable<PostsModel> GetYandexSimilarImages(List<GroupImagesAlike> similarImages = null, int limit = 10);
		IEnumerable<PostsModel> GetGoogleImages(string color, List<string> topics, List<string> sites, int limit = 10,
			string imageType = null, string exactSize = null, string similarImage = null);
	}
}

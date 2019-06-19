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
		string GenerateMediaInfo(TopicsModel topicSelect, string language);
		string GenerateText(string topic, string lang, int type, int limit, int size);
		Task<IEnumerable<string>> GetHashTags(string topic, int limit, int pickAmount);
		List<UserResponse> SearchInstagramUsersByTopic(UserStore user, string topic, int limit);
		Task<List<TopicsModel>> GetTopics(UserStore usersession, List<string> topic, int limit);
		bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime);
		IEnumerable<PostsModel> GetUserMedia(UserStore user, int limit = 1);
		IEnumerable<PostsModel> GetMediaInstagram(UserStore user, InstaMediaType mediaType, List<string> topics, int limit = 1);
		IEnumerable<PostsModel> GetYandexSimilarImages(List<GroupImagesAlike> similarImages = null, int limit = 10);
		IEnumerable<PostsModel> GetGoogleImages(string color, List<string> topics, List<string> sites, int limit = 10,
			string imageType = null, string exactSize = null, string similarImage = null);
	}
}

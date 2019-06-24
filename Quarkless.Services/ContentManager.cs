using InstagramApiSharp.Classes.Models;
using Quarkless.Services.ContentSearch;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.TextGeneration;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services
{
	public class ContentManager : IContentManager
	{
		private readonly ITopicBuilder _topicBuilder; 
		private readonly IContentSearch _contentSearch;
		private readonly ITextGeneration _textGeneration;
		private readonly ITimelineLogic _timelineLogic;
		private IUserStoreDetails _user { get; set; }
		public ContentManager(ITopicBuilder topicBuilder, ITimelineLogic timelineLogic, 
			IContentSearch contentSearch, ITextGeneration textGeneration)
		{
			_topicBuilder = topicBuilder;
			_timelineLogic = timelineLogic;
			_contentSearch = contentSearch;
			_textGeneration = textGeneration;
		}
		public void SetUser(IUserStoreDetails user)
		{
			_contentSearch.SetUserClient(user);
			_topicBuilder.Init(user);
			_user = user;
		}
		public async Task<List<TopicsModel>> GetTopics(List<string> topics,int limit)
		{
			List<TopicsModel> totalFound = new List<TopicsModel>();
			foreach(var topic in topics) { 
				var res = await _topicBuilder.Build(topic, limit);
				if(res!=null)
					totalFound.Add(res);
			}

			return totalFound;
		}
		public async Task<IEnumerable<string>> GetHashTags (string topic, int limit, int pickAmount)
		{
			return await _topicBuilder.BuildHashtags(topic,limit,pickAmount);
		}
		public string GenerateText(string topic,string lang, int type, int limit, int size)
		{
			return _textGeneration.MarkovTextGenerator(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\{0}.csv",
				type,topic,lang,size,limit) ;
		}
		public IEnumerable<PostsModel> GetMediaInstagram(InstaMediaType mediaType, List<string> topics, int limit = 1)
		{
			var medias = _contentSearch.SearchMediaInstagram(topics,mediaType,limit).GetAwaiter().GetResult();
			if(medias == null) return null;
			if(medias.Medias.Count<=0) return null;
			return medias.Medias.Select(i=> new PostsModel { MediaData = i.MediaUrl});
		}
		public IEnumerable<PostsModel> GetYandexSimilarImages(List<GroupImagesAlike> similarImages=null, int limit = 10) 
		{
			if (similarImages != null)
			{
				var relatedImages = _contentSearch.SearchViaYandexBySimilarImages(similarImages,limit);
				if(relatedImages==null) return null;
				if(relatedImages.Medias.Count <= 0)return null;

				return relatedImages.Medias.Select(y=>new PostsModel { MediaData = y.MediaUrl });
			}
			return null;
		}
		public IEnumerable<PostsModel> GetGoogleImages(string color, List<string> topics , List<string> sites, int limit = 10, 
			string imageType = null, string exactSize = null, string similarImage = null)
		{
			var query = new SearchImageModel
			{
				color = color,
				prefix_keywords = string.Join(", ", sites),
				keywords = string.Join(", ",topics),
				limit = limit,
				no_download = true,
				print_urls = true,
				type = imageType,
				exact_size = exactSize,
				proxy = null,
				related_images = similarImage,
				size = string.IsNullOrEmpty(exactSize) ? "large" : null
			};
			var relatedImages = _contentSearch.SearchViaGoogle(query);
			if (relatedImages == null) return null;
			if (relatedImages.Medias.Count <= 0) return null;
			return relatedImages.Medias.Select(g=>new PostsModel { MediaData = g.MediaUrl});
		}
		public string GenerateMediaInfo(TopicsModel topicSelect, string language)
		{
			var hash = GetHashTags(topicSelect.TopicName, 150, 30).GetAwaiter().GetResult().ToList();
			hash.AddRange(topicSelect.SubTopics.Select(s => $"#{s}"));
			var hashtags = hash.TakeAny(SecureRandom.Next(28)).JoinEvery(Environment.NewLine, 3);
			var caption_ = GenerateText(topicSelect.TopicName.ToLower(), language.ToUpper(), 1, 1, 1);
			return caption_ + Environment.NewLine + hashtags;
		}
		public IEnumerable<PostsModel> GetUserMedia(string userName = null, int limit = 1)
		{
			var results = _contentSearch.SearchMediaUser(userName,limit).GetAwaiter().GetResult();
			return results.Medias.Select(g=>new PostsModel { MediaData = g.MediaUrl});
		}
		public IEnumerable<UserResponse<MediaDetail>> SearchUsersMediaDetailInstagram(string userName, int limit)
		{
			try
			{
				return _contentSearch.SearchUsersMediaDetailInstagram(userName,limit).GetAwaiter().GetResult();
			}
			catch(Exception ee)
			{
				Console.Write(ee.Message);
				return null;
			}
		}
		public bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.User = _user as UserStore;
			return _timelineLogic.AddToTimeline(restBody,executeTime);
		}
		public IEnumerable<UserResponse<MediaDetail>> SearchMediaDetailInstagram(List<string> topics, int limit = 1)
		{
			var medias = _contentSearch.SearchMediaDetailInstagram(topics, limit).GetAwaiter().GetResult();
			if (medias.Count() <= 0) return null;
			return medias;
		}
		public InstaFullUserInfo SearchInstagramFullUserDetail(long userId)
		{
			try
			{
				return _contentSearch.SearchInstagramFullUserDetail(userId).GetAwaiter().GetResult();
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public List<UserResponse<string>> SearchInstagramMediaLikers(string mediaId)
		{
			try
			{
				return _contentSearch.SearchInstagramMediaLikers(mediaId).GetAwaiter().GetResult();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public List<UserResponse<CommentResponse>> SearchInstagramMediaCommenters(string mediaId, int limit)
		{
			try
			{
				return _contentSearch.SearchInstagramMediaCommenters(mediaId,limit).GetAwaiter().GetResult();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public IEnumerable<UserResponse<MediaDetail>> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1)
		{
			try
			{
				return _contentSearch.SearchUserFeedMediaDetailInstagram(seenMedias,requestRefresh).GetAwaiter().GetResult();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}

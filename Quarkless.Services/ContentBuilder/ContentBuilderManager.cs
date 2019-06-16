using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.ContentBuilder.ContentSearch;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.RequestBuilder;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.TextGeneration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Services.ContentBuilder
{
	public class ContentBuilderManager : IContentBuilderManager
	{
		//Build Topics
		//Based on those topics => Generate Media, Captions, Comments, Bio
		private readonly ITopicBuilder _topicBuilder; 
		private readonly IRequestBuilder _requestBuilder;
		private readonly ITaskService _taskService;
		private readonly IContentSearch _contentSearch;
		private readonly ITextGeneration _textGeneration;
		public ContentBuilderManager(IRequestBuilder requestBuilder, ITaskService taskService, 
			ITopicBuilder topicBuilder, IContentSearch contentSearch, ITextGeneration textGeneration)
		{
			_topicBuilder = topicBuilder;
			_taskService = taskService;
			_requestBuilder = requestBuilder;
			_contentSearch = contentSearch;
			_textGeneration = textGeneration;
		}

		public async Task<List<TopicsModel>> GetTopics(UserStore user, List<string> topics,int limit)
		{
			_topicBuilder.Init(user);
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

		public IEnumerable<PostsModel> GetMediaInstagram(UserStore user, InstaMediaType mediaType, List<string> topics, int limit = 1)
		{
			var medias = _contentSearch.SearchMediaInstagram(user,topics,mediaType,limit).GetAwaiter().GetResult();
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

		public bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders.AddRange(
				_requestBuilder.DefaultHeaders(
				restBody.User.InstaAccountId,
				restBody.User.AccessToken));

			_taskService.LongRunningTask(restBody, executeTime);
			return true;
		}
		public void GenerateContent(ContentType contentType)
		{
			// Need a way to get current instagram user
			// if topic does not exist in db add it
			// get topic and lists of topic
			//begin generating content

			//types of content
			/*
			 * Images	------> {		}
			 * Carousels ---> { Posts } These 3 also need to generate a caption
			 * Videos --------> {		}
			 * Stories -> might need to generate caption too
			 * Comments
			 * Bio
			 * Direct Messaging
			 * ITV 
			 */

			switch (contentType)
			{
				//user account needed and their instagram account (FOR ALL ACTIONS)
				case ContentType.Image:
					//params => user's profile (topic, theme), image size, and the actual image
					break;
				case ContentType.Video:
					//params => user's profile (topic,theme), video size, length, and the actual video
					break;
				case ContentType.Carousel:
					//params => user's profile (topic,theme), media items to add to carousels
					break;
				case ContentType.Comment:
					//params => user's profile (topic), mediaId, and the comment
					break;
				case ContentType.Bio:
					//params => user's profile (topic), the bio
					break;
				case ContentType.Story:
					//params => user's profile (topic, theme) and the media to upload 
					break;
				case ContentType.ITV:
					break;
				case ContentType.DirectMessage:
					break;
			}

		}
		public void AddActionToTimeLine(Delegate @delegate, DateTimeOffset executeTime,params object[] args)
		{
			_taskService.ActionTask(@delegate,executeTime,args);
		}
	}
}

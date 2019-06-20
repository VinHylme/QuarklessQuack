using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.RestSharpClient;
using QuarklessContexts.Models.Timeline;
using ContentSearcher;

namespace Quarkless.Services.ContentSearch
{
	public class ContentSearch : IContentSearch
	{
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IAPIClientContext _context;
		private readonly YandexImageSearch _yandexImageSearch;
		public ContentSearch(IRestSharpClientManager restSharpClient, IAPIClientContext clientContext)
		{
			_restSharpClient = restSharpClient;
			_context = clientContext;
			_yandexImageSearch = new YandexImageSearch();
		}

		public async Task<List<UserResponse>> SearchInstagramUsersByTopic(UserStore user, string topic, int limit)
		{
			IAPIClientContainer _container = new APIClientContainer(_context, user.AccountId, user.InstaAccountId);
			var res = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
			if (res.Succeeded)
			{
				List<UserResponse> users = res.Value.Medias.Select(_=>_?.User).Select(t=> new UserResponse 
				{
					UserId = t.Pk,
					Topic = topic,
					Username = t.UserName,
					FullName = t.FullName,
					IsVerified = t.IsVerified,
					IsPrivate = t.IsPrivate,
					FollowerCount = t.FollowersCount
				}).ToList();
				return users;
			}
			return null;
		}

		public async Task<IEnumerable<MediaDetail>> SearchMediaDetailInstagram(UserStore user, List<string> topics, int limit)
		{
			IAPIClientContainer _container = new APIClientContainer(_context, user.AccountId, user.InstaAccountId);
			List<MediaDetail> medias = new List<MediaDetail>();
			foreach(var topic in topics) { 
				var mediasResults = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (mediasResults.Succeeded)
				{
					medias.AddRange(mediasResults.Value.Medias.Select(s=> 
					{
						MediaDetail mediaDetail = new MediaDetail();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.Topic = topic;

						var totalurls = new List<string>();
						if(s.MediaType == InstaMediaType.Image)
						{
							totalurls.Add(s.Images.FirstOrDefault().Uri);
						}
						else if(s.MediaType == InstaMediaType.Video)
						{
							totalurls.Add(s.Videos.FirstOrDefault().Uri);
						}
						else if(s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.Select(x =>
							{
								var videos = x.Videos.FirstOrDefault().Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault().Uri;
								if (images != null)
									totalurls.Add(images);
								return s;
							});
						}
						mediaDetail.MediaUrl = totalurls;
						return mediaDetail;
					}));
				}
			}
			return medias;
		}

		public async Task<Media> SearchMediaInstagram(UserStore user, List<string> topics, InstaMediaType mediaType, int limit)
		{
			IAPIClientContainer _container = new APIClientContainer(_context,user.AccountId,user.InstaAccountId);
			Media mediaresp = new Media();
			foreach(var topic in topics) { 
				MediaResponse media_ = new MediaResponse();
				var results = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (results.Succeeded)
				{
					media_.Topic = topic;
					foreach(var results_media in results.Value.Medias)
					{
						switch (mediaType)
						{
							case InstaMediaType.All:
								var image = results_media.Images.FirstOrDefault().Uri;		
								if(image!=null)
									media_.MediaUrl.Add(image);
								var video = results_media.Videos.FirstOrDefault().Uri;
								if(video!=null)
									media_.MediaUrl.Add(video);
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault().Uri;
									if (videos != null)
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault().Uri;
									if (images != null)
										media_.MediaUrl.Add(images);
									return s;
								});
								break;
							case InstaMediaType.Image:
								media_.MediaUrl.Add(results_media.Images.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Video:
								media_.MediaUrl.Add(results_media.Videos.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Carousel:
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault().Uri;
									if (videos != null)
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault().Uri;
									if (images != null)
										media_.MediaUrl.Add(images);
									return s;
								});
								break;
						}
					}
					mediaresp.Medias.Add(media_);		
				}
			}
			return mediaresp;
		}
		public async Task<Media> SearchMediaUser(UserStore user, int limit = 1)
		{
			IAPIClientContainer _container = new APIClientContainer(_context, user.AccountId, user.InstaAccountId);
			Media mediaresp = new Media();
			var results = await _container.User.GetUserMediaAsync(_container.GetContext.InstagramAccount.Username, PaginationParameters.MaxPagesToLoad(limit));
			if (results.Succeeded)
			{
				MediaResponse media = new MediaResponse();
				foreach(var lema in results.Value)
				{
					switch (lema.MediaType)
					{
						case InstaMediaType.Image:
							media.MediaUrl.Add(lema.Images.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Video:
							media.MediaUrl.Add(lema.Videos.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Carousel:
							lema.Carousel.Select(s =>
							{
								var videos = s.Videos.FirstOrDefault().Uri;
								if(videos!=null)
									media.MediaUrl.Add(videos);
								var images = s.Images.FirstOrDefault().Uri;
								if (images!=null)
									media.MediaUrl.Add(images);
								return s;
							});
							break;
					}		
				}
				mediaresp.Medias.Add(media);
			}
			return mediaresp;
		}

		/// <summary>
		/// search images via google
		/// todo, add private url
		/// </summary>
		/// <param name="searchImageQuery"></param>
		/// <returns></returns>
		public Media SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var results = _restSharpClient.PostRequest("http://127.0.0.1:5000/","searchImages",JsonConvert.SerializeObject(searchImageQuery),null);
			if (results.IsSuccessful)
			{
				Media responseValues = JsonConvert.DeserializeObject<Media>(results.Content);
				return responseValues;
			}
			return null;
		}
		public Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit)
		{
			var images =  _yandexImageSearch.Search(imagesSimilarUrls, limit);
			return images;
		}
	}
}

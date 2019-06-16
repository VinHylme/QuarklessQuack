using ContentSearcher;
using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.RestSharpClient;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Services.Extensions;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.ContentBuilder.ContentSearch
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
					foreach(var results_media in results.Value.Medias.Where(a=>a.MediaType == mediaType))
					{
						switch (mediaType)
						{
							case InstaMediaType.Image:
								var tm = results_media.Images.Select(s => s.Uri).ToList();
								if(tm.Count>0)
									media_.MediaUrl.AddRange(tm);
								break;
							case InstaMediaType.Video:
								var tv = results_media.Videos.Select(s => s.Uri).ToList();
								if(tv.Count>0)
									media_.MediaUrl.AddRange(tv);
								break;
							case InstaMediaType.Carousel:
								List<string> total_ = new List<string>();
								results_media.Carousel.Select(s=> 
								{
									var videos = s.Videos.Select(sa=>sa.Uri).ToList();
									var images = s.Images.Select(sa=>sa.Uri).ToList();
									if(videos.Count>0)
										total_.AddRange(videos);
									if(images.Count>0)
										total_.AddRange(images);
									return s;
								});
								if(total_.Count>0)
									media_.MediaUrl.AddRange(total_);
								break;
						}
					}
					mediaresp.Medias.Add(media_);		
				}
			}
			return mediaresp;
		}

		public async Task<Media> SearchMediaUser(UserStore user, int limit = 1, params string[] instagramAccounts)
		{
			IAPIClientContainer _container = new APIClientContainer(_context, user.AccountId, user.InstaAccountId);
			Media mediaresp = new Media();
			foreach(var account in instagramAccounts)
			{
				var results = await _container.User.GetUserMediaAsync(account,PaginationParameters.MaxPagesToLoad(limit));
				if (results.Succeeded)
				{
					MediaResponse media = new MediaResponse();
					foreach(var lema in results.Value)
					{
						media.MediaUrl.AddRange(lema.Images.Select(s=>s.Uri));
						media.MediaUrl.AddRange(lema.Videos.Select(s=>s.Uri));
					
					}
					mediaresp.Medias.AddRange(results.Value.Select(m=>new MediaResponse{ }));
				}
			}
			return null;
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

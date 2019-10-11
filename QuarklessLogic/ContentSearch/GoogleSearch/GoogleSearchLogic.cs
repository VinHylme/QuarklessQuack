using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuarklessContexts.Models.Options;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.RestSharpClient;

namespace QuarklessLogic.ContentSearch.GoogleSearch
{
	public struct TempMedia
	{
		public struct Medias
		{
			public string Topic { get; set; }
			public string MediaUrl { get; set; }
		}
		public List<Medias> MediasObject;
		public int errors { get; set; }
	}

	public class GoogleSearchLogic : IGoogleSearchLogic
	{
		private readonly string _apiEndpoint;
		private readonly IRestSharpClientManager _restSharpClient;

		public GoogleSearchLogic(IOptions<GoogleSearchOptions> options)
		{
			_apiEndpoint = options.Value.Endpoint;
			_restSharpClient = new RestSharpClientManager();
		}

		public void WithProxy(ProxyModel proxy) => _restSharpClient.AddProxy(proxy);
		public SearchResponse<Media> SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var response = new SearchResponse<Media>();
			try
			{
				var results = _restSharpClient.PostRequest(_apiEndpoint, 
					"searchImages", JsonConvert.SerializeObject(searchImageQuery), null);

				if (results == null)
				{
					response.StatusCode = ResponseCode.InternalServerError;
					return response;
				}

				if (results.IsSuccessful)
				{
					var responseValues = JsonConvert.DeserializeObject<TempMedia>(results.Content);
					if (responseValues.MediasObject.Count <= 0)
					{
						response.StatusCode = ResponseCode.InternalServerError;
						response.Message = $"Google search returned no results for object: {JsonConvert.SerializeObject(searchImageQuery)}";
						return response;
					}

					var responseResult = new Media
					{
						Medias = responseValues.MediasObject.Select(s => new MediaResponse
						{
							Topic = s.Topic,
							MediaFrom = MediaFrom.Google,
							MediaType = InstaMediaType.Image,
							MediaUrl = new List<string> { s.MediaUrl }
						}).ToList()
					};

					response.StatusCode = ResponseCode.Success;
					response.Result = responseResult;
					return response;
				}
			}
			catch (Exception ee)
			{
				response.Message = ee.Message;
				response.StatusCode = ResponseCode.InternalServerError;
				return response;
			}
			response.StatusCode = ResponseCode.ReachedEndAndNull;
			response.Message = $"SearchViaGoogle failed for  object{JsonConvert.SerializeObject(searchImageQuery)}";
			return response;
		}
		public SearchResponse<Media> SearchSimilarImagesViaGoogle(List<GroupImagesAlike> groupImages, int limit, int offset = 0)
		{
			var response = new SearchResponse<Media>();
			try
			{
				foreach (var images in groupImages)
				{
					var searchImage = new SearchImageModel
					{
						no_download = true,
						similar_images = images.Url,
						limit = limit,
						offset = offset < limit ? offset : 0
					};
					var res = _restSharpClient.PostRequest(_apiEndpoint,
						"searchImages", JsonConvert.SerializeObject(searchImage));

					var responseValues = JsonConvert.DeserializeObject<TempMedia>(res.Content);
					if (responseValues.MediasObject.Count <= 0)
					{
						response.StatusCode = ResponseCode.InternalServerError;
						response.Message = $"Google search returned no results for object: {JsonConvert.SerializeObject(searchImage)}";
						return response;
					}

					response.Result.Medias.AddRange(responseValues.MediasObject.Select(s => new MediaResponse
					{
						Topic = images.TopicGroup,
						MediaFrom = MediaFrom.Google,
						MediaType = InstaMediaType.Image,
						MediaUrl = new List<string> { s.MediaUrl }
					}).ToList());
				}

				response.StatusCode = ResponseCode.Success;
				return response;
			}
			catch (Exception ee)
			{
				response.Message = ee.Message;
				response.StatusCode = ResponseCode.InternalServerError;
				return response;
			}
		}
	}
}

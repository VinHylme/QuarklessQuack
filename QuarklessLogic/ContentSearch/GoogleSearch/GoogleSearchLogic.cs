using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenQA.Selenium;
using QuarklessContexts.Models.Options;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.ContentSearch.SeleniumClient;
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
		private const string IMAGE_URL = "https://www.google.co.uk/imghp?hl=en&tab=wi&ogbl";
		private readonly string _apiEndpoint;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly ISeleniumClient _seleniumClient;

		public GoogleSearchLogic(IOptions<GoogleSearchOptions> options, ISeleniumClient seleniumClient)
		{
			_seleniumClient = seleniumClient;
			_apiEndpoint = options.Value.Endpoint;
			_restSharpClient = new RestSharpClientManager();
		}

		public void WithProxy(ProxyModel proxy)
		{
			_restSharpClient.AddProxy(proxy);
			var proxyLine = string.IsNullOrEmpty(proxy.Username) ? $"http://{proxy.Address}:{proxy.Port}" : $"http://{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
			_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
		}
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
		public SearchResponse<Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit, int offset = 0)
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

		public async Task<IEnumerable<string>> GetSuggestions(string query)
		{
			var results = new List<string>();
			using (var driver = _seleniumClient.CreateDriver())
			{
				driver.Navigate().GoToUrl(IMAGE_URL);
				var searchBox = driver.FindElement(By.XPath("//input[@title= 'Search']"));
				await Task.Delay(TimeSpan.FromSeconds(.25));
				searchBox.SendKeys(query);
				searchBox.Submit();
				await Task.Delay(TimeSpan.FromSeconds(.55));
				var carouselSuggestions = driver.FindElements(By.ClassName("dtviD"));
				if(carouselSuggestions.Any())
					results.AddRange(carouselSuggestions.Select(_=>_.Text));
			}

			return results;
		}

	}
}

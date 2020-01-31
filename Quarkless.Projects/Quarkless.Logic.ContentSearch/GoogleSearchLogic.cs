using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.SearchResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.SearchResponse.Enums;
using Quarkless.Models.SearchResponse.Structs;
using Newtonsoft.Json;
using Quarkless.Models.Proxy;
using Quarkless.Models.SeleniumClient.Interfaces;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.Topic;

namespace Quarkless.Logic.ContentSearch
{
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
			_restSharpClient = new RestSharpClientManager.RestSharpClientManager();
		}

		public IGoogleSearchLogic WithProxy(ProxyModel proxy = null)
		{
			//TODO: Add proxy rotating service and assign a proxy
			if (proxy == null)
			{

			}
			else
			{
				_restSharpClient.AddProxy(proxy);
				var proxyLine = string.IsNullOrEmpty(proxy.Username)
					? $"http://{proxy.HostAddress}:{proxy.Port}"
					: $"http://{proxy.Username}:{proxy.Password}@{proxy.HostAddress}:{proxy.Port}";
				_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}
			return this;
		}
		public SearchResponse<Media> SearchViaGoogle(CTopic topic, SearchImageModel searchImageQuery)
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
							Topic = topic,
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
						NoDownload = true,
						SimilarImages = images.Url,
						Limit = limit,
						Offset = offset < limit ? offset : 0
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
						Topic = null,
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
		public async Task<List<string>> GetSuggestions(string query)
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
				if (carouselSuggestions.Any())
					results.AddRange(carouselSuggestions.Select(_ => _.Text));
			}

			return results;
		}
	}
}

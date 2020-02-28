using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.SearchResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.ContentSearch.Enums;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.Topic;

namespace Quarkless.Logic.ContentSearch
{
	public class GoogleSearchLogic : IGoogleSearchLogic
	{
		private const string IMAGE_URL = "https://www.google.co.uk/search?";
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
		private string QueryBuilder(SearchGoogleImageRequestModel query)
		{
			var queryBuilder = IMAGE_URL;
			var keywordBuilder = string.Empty;

			if (!string.IsNullOrEmpty(query.Prefix))
				keywordBuilder += $"{query.Prefix}+";
			keywordBuilder += query.Keyword;
			if (!string.IsNullOrEmpty(query.Suffix))
				keywordBuilder += $"+{query.Suffix}";

			queryBuilder += $"q={keywordBuilder}&tbm=isch&hl=en&hl=en";
			const string specificTag = "&tbs=ic:specific";
			queryBuilder += specificTag;

			if (query.Color != ColorType.Any)
			{
				queryBuilder += $",isc:{query.ColorForGoogle}";
			}

			if (query.MediaType != ImageType.Any)
			{
				queryBuilder += $",itp:{query.MediaType.GetDescription()}";
			}

			queryBuilder += $",isz:{query.CorrectSizeTypeFormat}";

			queryBuilder += "&biw=1504&bih=734";
			return queryBuilder;
		}
		public async Task<SearchResponse<Media>> SearchGoogleImages(CTopic topic, SearchGoogleImageRequestModel searchQuery)
		{
			var response = new SearchResponse<Media>();
			if (topic == null)
			{
				response.StatusCode = ResponseCode.InternalServerError;
				response.Message = "Topic was null";
				return response;
			}

			if (searchQuery == null || string.IsNullOrEmpty(searchQuery.Keyword))
			{
				response.StatusCode = ResponseCode.InternalServerError;
				response.Message = "Invalid search query";
				return response;
			}

			try
			{
				var request = QueryBuilder(searchQuery);
				response.Result = new Media();
				using var client = _seleniumClient.CreateDriver();
				client.Navigate().GoToUrl(request);
				var totalMedias = new List<MediaResponse>();

				var elements = client.FindElements(By.XPath("//a[@class='wXeWr islib nfEiy mM5pbd']"));
				if (elements.Any())
				{
					foreach (var webElement in elements.Take(searchQuery.Limit))
					{
						try
						{
							webElement.Click();
							await Task.Delay(TimeSpan.FromSeconds(.5));
							var image = client.FindElements(By.ClassName("n3VNCb"));
							var srcUrl = image.FirstOrDefault(_ => !_.GetProperty("src").Contains("data:"));
							if (srcUrl == null) continue;

							var src = srcUrl.GetProperty("src");

							if(src.Contains(".gif")) continue;
							
							var title = client.FindElement(By.ClassName("Beeb4e"))?.Text;

							totalMedias.Add(new MediaResponse
							{
								MediaUrl = new[] { src }.ToList(),
								MediaId = src.ToByteArray().ComputeHash().ToString(),
								MediaType = InstaMediaType.Image,
								Caption = title,
								MediaFrom = MediaFrom.Google,
								Topic = topic
							});

							#region Commented code for getting related images too
							/*
							// for (var x = 0; x < 5; x++)
							// {
							// 	var elementsRelated = client.FindElements(By.XPath("//a[@class='wXeWr islib nfEiy mM5pbd']"))
							// 		.TakeLast(12);
							//
							// 	var element = elementsRelated.ElementAt(x);
							//
							// 	try
							// 	{
							// 		element.Click();
							// 		await Task.Delay(TimeSpan.FromSeconds(.95));
							// 		var imageR = client.FindElements(By.ClassName("n3VNCb"));
							// 		var srcUrlR = imageR.FirstOrDefault(_ => !_.GetProperty("src").Contains("data:"));
							// 		if (srcUrlR == null) continue;
							//
							// 		var srcR = srcUrlR.GetProperty("src");
							// 		var titleR = client.FindElement(By.ClassName("Beeb4e"))?.Text;
							//
							// 		totalMedias.Add(new MediaResponse
							// 		{
							// 			MediaUrl = new[] { srcR }.ToList(),
							// 			Caption = titleR,
							// 			MediaFrom = MediaFrom.Google,
							// 			Topic = topic
							// 		});
							//
							// 		client.Navigate().Back();
							// 	}
							// 	catch(Exception err)
							// 	{
							// 		Console.WriteLine("failed to get image");
							// 	}
							// }
							*/
							#endregion
							var closeButton = client.FindElement(By.ClassName("hm60ue"));
							closeButton.Click();
						}
						catch(Exception err)
						{
							Console.WriteLine("could not load this image");
						}
					}
				}
				
				if(totalMedias.Any())
					response.Result.Medias.AddRange(totalMedias);

				response.StatusCode = ResponseCode.Success;
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				response.StatusCode = ResponseCode.InternalServerError;
				response.Message = err.Message;
				return response;
			}
		}

		public SearchResponse<Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit, int offset = 0)
		{
			return new SearchResponse<Media>
			{
				StatusCode = ResponseCode.InternalServerError
			};
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

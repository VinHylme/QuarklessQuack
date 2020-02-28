using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.ContentSearch.Enums;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.ContentSearch.Models.Yandex;
using Quarkless.Models.Proxy;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.SearchResponse.Enums;
using Quarkless.Models.SeleniumClient.Interfaces;
namespace Quarkless.Logic.ContentSearch
{
	public class YandexImageSearch : IYandexImageSearch
	{

		#region URL CONSTANTS
		private const string yandexImages = @"https://yandex.com/images/";
		private const string yandexBaseImageUrl = @"https://yandex.com/images/search?url=";
		private const string rpt = @"&rpt=imagelike";
		private const string yandexUrl = @"https://yandex.com";
		//private const string queryTypeId = "jQuery21407858805378890188_1563481544251";
		//private const string imsearchTypeId = "jQuery21404334140969556852_1563358335242";

		private const string ajaxCallBackUrl = @"https://yandex.com/images/search?callback=jQuery21407858805378890188_{0}&format=json&request=";
		private const string uinfo_ = @"sw-1920-sh-1080-ww-1745-wh-855-pd-1.100000023841858-wp-16x9_2560x1440&serpid={0}&serpListType=horizontal&_=1563358335246";
		#endregion
		private readonly object _locker = new object();
		private readonly ISeleniumClient _seleniumClient;
		private readonly IRestSharpClientManager _restSharpClientManager;

		public YandexImageSearch(ISeleniumClient seleniumClient)
		{
			_restSharpClientManager = new RestSharpClientManager.RestSharpClientManager();

			_seleniumClient = seleniumClient;
			_seleniumClient.AddArguments(
				"headless",
				"--ignore-certificate-errors");
		}

		public IYandexImageSearch WithProxy(ProxyModel proxy = null)
		{
			//TODO: ADD ROTATING PROXY CODE AND ASSIGN A PROXY
			if (proxy == null)
			{

			}
			else
			{
				_restSharpClientManager.AddProxy(proxy);
				var proxyLine = string.IsNullOrEmpty(proxy.Username)
					? $"http://{proxy.HostAddress}:{proxy.Port}"
					: $"http://{proxy.Username}:{proxy.Password}@{proxy.HostAddress}:{proxy.Port}";
				_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}
			return this;
		}
		private string BuildUrl(YandexSearchQuery yandexSearch)
		{
			var urlBuilt = string.Empty;
			if (string.IsNullOrEmpty(yandexSearch.SearchQuery)) return null;
			urlBuilt += $"search?text={HttpUtility.UrlEncode(yandexSearch.SearchQuery)}";
			if (yandexSearch.Orientation != Orientation.Any)
			{
				urlBuilt += "&iorient=" + yandexSearch.Orientation.GetDescription();
			}
			if (yandexSearch.Type != ImageType.Any)
			{
				urlBuilt += "&type=" + yandexSearch.Type.GetDescription();
			}
			if (yandexSearch.Color != ColorType.Any)
			{
				urlBuilt += "&icolor=" + yandexSearch.ColorForYandex;
			}
			if (yandexSearch.Format != FormatType.Any)
			{
				urlBuilt += "&itype=" + yandexSearch.Format.GetDescription();
			}
			if (yandexSearch.Size != SizeType.None && yandexSearch.SpecificSize == null)
			{
				urlBuilt += "&isize=" + yandexSearch.CorrectSizeTypeFormat;
			}
			if (yandexSearch.SpecificSize != null && yandexSearch.Size == SizeType.None)
			{
				urlBuilt += "&isize=eq&iw=" + yandexSearch.SpecificSize.Value.Width + "&ih=" + yandexSearch.SpecificSize.Value.Height;
			}
			return yandexImages + urlBuilt;
		}
		public SearchResponse<Media> SearchQueryRest(YandexSearchQuery yandexSearchQuery, int limit = 1)
		{
			var response = new SearchResponse<Media>();
			var totalFound = new Media();
			try
			{
				var url = BuildUrl(yandexSearchQuery);
				var result = Reader(url, limit);

				if (result?.Result != null)
				{
					foreach (var item in result.Result)
					{
						var mediaUrl = item?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url;
						if(mediaUrl == null) continue;
						if(mediaUrl.Contains(".gif")) continue;
						var mediaId = mediaUrl.ToByteArray().ComputeHash().ToString();
						if(totalFound.Medias.Exists(_=>_.MediaId == mediaId)) continue;

						totalFound.Medias.Add(new MediaResponse
						{
							Topic = yandexSearchQuery.OriginalTopic,
							MediaId = mediaId,
							MediaType = InstaMediaType.Image,
							MediaFrom = MediaFrom.Yandex,
							MediaUrl = new List<string>(new[]{ mediaUrl }),
							Caption = item.Snippet?.Text,
							Title = item.Snippet?.Title,
							Domain = item.Snippet?.Domain
						});
					}
				}

				return new SearchResponse<Media>
				{
					Message = result?.Message,
					Result = totalFound,
					StatusCode = result?.StatusCode ?? ResponseCode.InternalServerError
				};
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				response.StatusCode = ResponseCode.InternalServerError;
				response.Message = ee.Message;
				return response;
			}
		}
		public SearchResponse<Media> SearchSafeButSlow(IEnumerable<GroupImagesAlike> similarImages, int limit)
		{
			var totalFound = new Media();
			var response = new SearchResponse<Media>();

			similarImages.ToList().ForEach(url =>
			{
				if (url == null) return;

				var httpsYandexComImages = yandexImages;

				try
				{
					var result = ImageSearch(httpsYandexComImages, url.Url, "serp-item_pos_", limit);
					foreach (var mediaUrl in result)
					{
						if(mediaUrl == null) continue;
						if(mediaUrl.Contains(".gif")) continue;
						var mediaId = mediaUrl.ToByteArray().ComputeHash().ToString();
						if(totalFound.Medias.Exists(_=>_.MediaId == mediaId)) continue;

						totalFound.Medias.Add(new MediaResponse
						{
							Topic = url.TopicGroup,
							MediaId = mediaId,
							MediaType = InstaMediaType.Image,
							MediaFrom = MediaFrom.Yandex,
							MediaUrl = new List<string>(new[]{ mediaUrl }),
						});
					}
				}
				catch (Exception ee)
				{
					Console.Write(ee.Message);
					response.Message = ee.Message;
					response.StatusCode = ResponseCode.InternalServerError;
					totalFound.Errors++;
				}
			});

			if (totalFound.Medias.Count > 0)
			{
				response.Result = totalFound;
				response.StatusCode = ResponseCode.Success;
			}
			else
			{
				response.Result = null;
				response.StatusCode = ResponseCode.ReachedEndAndNull;
			}
			return response;
		}

		public SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages)
		{
			return YandexSearchMe(imageUrl, numberOfPages);
		}

		public SearchResponse<Media> SearchRelatedImagesRest(IEnumerable<GroupImagesAlike> similarImages,
			int numberOfPages)
		{
			var response = new SearchResponse<Media>();
			var totalCollected = new Media();

			foreach (var url in similarImages)
			{
				if (url == null) continue;
				var fullImageUrl = yandexBaseImageUrl + url.Url + rpt;

				try
				{
					var searchItems = SearchRest(fullImageUrl, numberOfPages);

					if (searchItems.StatusCode == ResponseCode.Success
						|| (searchItems.StatusCode == ResponseCode.CaptchaRequired && searchItems?.Result?.Count > 0))
					{
						foreach (var item in searchItems.Result)
						{
							var mediaUrl = item?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url;
							if (mediaUrl == null) continue;
							if (mediaUrl.Contains(".gif")) continue;
							var mediaId = mediaUrl.ToByteArray().ComputeHash().ToString();
							if (totalCollected.Medias.Exists(_ => _.MediaId == mediaId)) continue;

							totalCollected.Medias.Add(new MediaResponse
							{
								Topic = url.TopicGroup,
								MediaId = mediaId,
								MediaType = InstaMediaType.Image,
								MediaFrom = MediaFrom.Yandex,
								MediaUrl = new List<string>(new[] { mediaUrl }),
								Caption = item.Snippet?.Text,
								Title = item.Snippet?.Title,
								Domain = item.Snippet?.Domain
							});
						}
					}
					else
					{
						response.StatusCode = searchItems.StatusCode;
						response.Message = searchItems.Message;
						return response;
					}
				}
				catch (Exception ee)
				{
					response.Message = ee.Message;
					response.StatusCode = ResponseCode.InternalServerError;
					return response;
				}
			}

			if (totalCollected.Medias.Count > 0)
			{
				response.Result = totalCollected;
				response.StatusCode = ResponseCode.Success;
			}
			else
			{
				response.Result = null;
				response.StatusCode = ResponseCode.ReachedEndAndNull;
			}
			return response;
		}
		public SearchResponse<List<SerpItem>> YandexSearchMe(string url, int pages)
		{
			try
			{
				var totalCollected = new List<SerpItem>();
				var response = new SearchResponse<List<SerpItem>>();
				using (var driver = _seleniumClient.CreateDriver())
				{
					driver.Navigate().GoToUrl(url);
					if (driver.Url.Contains("captcha"))
					{
						response.StatusCode = ResponseCode.CaptchaRequired;
						response.Message = "Yandex captcha needed";
						return response;
					}

					totalCollected.AddRange(ReadSerpItems(driver, pages));
				}

				response.StatusCode = ResponseCode.Success;
				response.Result = totalCollected;
				return response;
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message);
				return null;
			}
		}
		public IEnumerable<string> ImageSearch(string url, string imageurl, string targetElement,
			int limit = 5, string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)")
		{
			try
			{
				var urls = new List<string>();
				using var driver = _seleniumClient.CreateDriver();
				driver.Navigate().GoToUrl(url);

				var searchButton = driver.FindElement(By.ClassName("input__button"));
				//TEXTCOPY WAS HERE
				searchButton.Click();
				var searchField = driver.FindElement(By.Name("cbir-url"));
				searchField.SendKeys(imageurl);
				var submitButton = driver.FindElement(By.Name("cbir-submit"));
				submitButton.Click();

				var pagehere = driver.PageSource;
				Thread.Sleep(2000);

				var similarButton = driver.FindElement(By.ClassName("similar__link"));
				//var hrefval = similarButton.GetAttribute("href");

				((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", similarButton);
				Thread.Sleep(3000);
				driver.FindElement(By.ClassName("pane2__close-icon")).Click();
				var elements = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

				while (elements.Count < limit)
				{
					_seleniumClient.ScrollPage(driver, 1);
					var uniqueFind = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]"));
					elements.AddRange(uniqueFind);
					elements = elements.Distinct().ToList();
				}

				for (var x = 0; x <= limit; x++)
				{
					var tohtml = elements[x].GetAttribute("outerHTML");

					var decoded = HttpUtility.HtmlDecode(tohtml);
					var imageUrl = Regex.Matches(decoded, patternRegex).Select(_ => _.Value).FirstOrDefault();
					if (string.IsNullOrEmpty(imageUrl))
					{
						if (x > elements.Count)
						{
							throw new Exception("can't find any images, all returning null");
						}
						x = x == 0 ? 0 : x--;
					}
					else
					{
						urls.Add(imageUrl);
					}
				}
				return urls.TakeAny(SecureRandom.Next(urls.Count));
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		private IEnumerable<SerpItem> ReadSerpItems(IWebDriver driver, int pageLimit = 2)
		{
			var total = new List<SerpItem>();
			_seleniumClient.ScrollPageByPixel(driver, pageLimit);

			var source = driver.PageSource.Replace("&quot;", "\"");
			var regexMatch = Regex.Matches(source, "{\"serp-item\":.*?}}");
			var results = new List<string>();

			foreach (Match x in regexMatch)
			{
				var newRes = x.Value.Replace("{\"serp-item\":", "");
				results.Add(newRes.Substring(0, newRes.Length - 1));
			}

			if (results.Count <= 0)
				return total;

			foreach (var value in results.Distinct())
			{
				try
				{
					total.Add(JsonConvert.DeserializeObject<SerpItem>(value));
				}
				catch
				{
					var tryAgain = value + "}}";
					total.Add(JsonConvert.DeserializeObject<SerpItem>(tryAgain));
				}
			}

			if (total.Count > 0)
				total.RemoveAt(0);

			return total;
		}
		public SearchResponse<List<SerpItem>> Reader(string url, int limit = 1)
		{
			try
			{
				lock (_locker)
				{
					var totalCollected = new List<SerpItem>();
					var response = new SearchResponse<List<SerpItem>>();

					using (var driver = _seleniumClient.CreateDriver())
					{
						driver.Navigate().GoToUrl(url);
						if (driver.Url.Contains("captcha"))
						{
							response.StatusCode = ResponseCode.CaptchaRequired;
							response.Message = "Yandex captcha needed";
							return response;
						}

						Thread.Sleep(500);
						try
						{
							var misspell = driver.FindElement(By.ClassName("misspell__message"));
							if (misspell != null)
							{
								var autosuggestion = "https://yandex.com" +
								 Regex.Match(misspell.GetAttribute("outerHTML"),
										 "href=.*?/images.*?>").Value.Replace("href=", "")
									 .Replace(">", "").Replace("\"", "");
								driver.Navigate().GoToUrl(autosuggestion);
								Thread.Sleep(2000);
							}
						}
						catch
						{
							Console.WriteLine("nothing suggested for yandex search query");
						}

						totalCollected.AddRange(ReadSerpItems(driver, limit));
					}
					response.StatusCode = ResponseCode.Success;
					response.Result = totalCollected;
					return response;
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee);
				return null;
			}
		}
	}
}
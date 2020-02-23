using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using OpenQA.Selenium;
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
				urlBuilt += "&icolor=" + yandexSearch.Color.GetDescription();
			}
			if (yandexSearch.Format != FormatType.Any)
			{
				urlBuilt += "&itype=" + yandexSearch.Format.GetDescription();
			}
			if (yandexSearch.Size != SizeType.None && yandexSearch.SpecificSize == null)
			{
				urlBuilt += "&isize=" + yandexSearch.Size.GetDescription();
			}
			if (yandexSearch.SpecificSize != null && yandexSearch.Size == SizeType.None)
			{
				urlBuilt += "&isize=eq&iw=" + yandexSearch.SpecificSize.Value.Width + "&ih=" + yandexSearch.SpecificSize.Value.Height;
			}
			return yandexImages + urlBuilt;
		}
		public SearchResponse<Media> SearchQueryRest(YandexSearchQuery yandexSearchQuery, int limit = 16)
		{
			var response = new SearchResponse<Media>();
			var totalFound = new Media();
			try
			{
				var url = BuildUrl(yandexSearchQuery);
				var result = Reader(url, limit);
				if (result?.Result != null)
				{
					totalFound.Medias.AddRange(result.Result.Select(o => new MediaResponse
					{
						Topic = yandexSearchQuery.OriginalTopic,
						MediaId = o?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url.ToByteArray().ComputeHash().ToString(),
						MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image,
						MediaFrom = MediaFrom.Yandex,
						MediaUrl = new List<string> { o?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url },
						Caption = o?.Snippet?.Text,
						Title = o?.Snippet?.Title,
						Domain = o?.Snippet?.Domain
					}));
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
					totalFound.Medias.AddRange(result.Where(s => !s.Contains(".gif")).Select(a => new MediaResponse
					{
						Topic = url.TopicGroup,
						MediaId = a.ToByteArray().ComputeHash().ToString(),
						MediaUrl = new List<string> { a },
						MediaFrom = MediaFrom.Yandex,
						MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image
					}));
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
		public SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages, int offset = 0)
		{
			return YandexSearchMe(imageUrl, numberOfPages, offset);
		}
		public SearchResponse<Media> SearchRelatedImagesRest(IEnumerable<GroupImagesAlike> similarImages, int numberOfPages, int offset = 0)
		{
			var response = new SearchResponse<Media>();
			var totalCollected = new Media();

			foreach (var url in similarImages)
			{
				if (url == null) continue;
				var fullImageUrl = yandexBaseImageUrl + url.Url + rpt;
				try
				{
					var searchSerp = SearchRest(fullImageUrl, numberOfPages, offset);
					if (searchSerp.StatusCode == ResponseCode.Success || (searchSerp.StatusCode == ResponseCode.CaptchaRequired && searchSerp?.Result?.Count > 0))
					{
						totalCollected.Medias.AddRange(searchSerp.Result.Select(o => new MediaResponse
						{
							MediaId = o?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url.ToByteArray().ComputeHash().ToString(),
							MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image,
							MediaFrom = MediaFrom.Yandex,
							MediaUrl = new List<string> { o?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url },
							Caption = o?.Snippet?.Text,
							Title = o?.Snippet?.Title,
							Domain = o?.Snippet?.Domain
						}));
					}
					else
					{
						response.StatusCode = searchSerp.StatusCode;
						response.Message = searchSerp.Message;
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
		public SearchResponse<List<SerpItem>> YandexSearchMe(string url, int pages, int offset = 0)
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

					totalCollected.AddRange(ReadSerpItems(driver, pages, offset));
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
		public IEnumerable<string> ImageSearch(string url, string imageurl, string targetElement, int limit = 5, string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)")
		{
			try
			{
				var urls = new List<string>();
				using (var driver = _seleniumClient.CreateDriver())
				{
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
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		private IEnumerable<SerpItem> ReadSerpItems(IWebDriver driver, int pageLimit, int offset = 0)
		{
			var total = new List<SerpItem>();
			for (var currPage = offset; currPage <= pageLimit; currPage++)
			{
				var source = driver.PageSource.Replace("&quot;", "\"");
				var regexMatch = Regex.Matches(source, "{\"serp-item\":.*?}}");
				var results = new List<string>();
				foreach (Match x in regexMatch)
				{
					var newRes = x.Value.Replace("{\"serp-item\":", "");
					results.Add(newRes.Substring(0, newRes.Length - 1));
				}
				if (results.Count <= 0)
				{
					break;
				}

				foreach (var serpString in results)
				{
					try
					{
						total.Add(JsonConvert.DeserializeObject<SerpItem>(serpString));
					}
					catch
					{
						var tryagainSerpString = serpString + "}}";
						total.Add(JsonConvert.DeserializeObject<SerpItem>(tryagainSerpString));
						//Console.WriteLine("could not convert serp object");
					}
				}
				if (total.Count > 0)
					total.RemoveAt(0); //remove the duplicate

				var nextPageUrl = driver.FindElement(By.ClassName("more__button")).GetAttribute("href");
				Thread.Sleep(1000);
				driver.Navigate().GoToUrl(nextPageUrl);
			}
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
		public IEnumerable<string> YandexImageSearchREST(string baseurl, string url, int pageLimit = 5)
		{
			try
			{
				using (var driver = _seleniumClient.CreateDriver())
				{
					driver.Navigate().GoToUrl(baseurl);
					var souce = driver.PageSource;
					driver.Navigate().GoToUrl(url);
					var manageD = driver.Manage().Cookies.AllCookies;
					var source = driver.PageSource;
				}

				return null;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}
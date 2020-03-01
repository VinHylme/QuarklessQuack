using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Logic.PuppeteerClient;
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
namespace Quarkless.Logic.ContentSearch
{
	public class YandexImageSearch : IYandexImageSearch
	{
		#region URL CONSTANTS
		private const string YANDEX_IMAGES = @"https://yandex.com/images/";
		private const string YANDEX_BASE_IMAGE_URL = @"https://yandex.com/images/search?url=";
		private const string RPT = @"&rpt=imagelike";
		#endregion

		private readonly IRestSharpClientManager _restSharpClientManager;
		private readonly IPuppeteerClient _puppeteerClient;
		public YandexImageSearch(IPuppeteerClient puppeteerClient)
		{
			_restSharpClientManager = new RestSharpClientManager.RestSharpClientManager();
			_puppeteerClient = puppeteerClient;
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
			}
			return this;
		}

		private string QueryUrlBuilder(YandexSearchQuery yandexSearch)
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
			return YANDEX_IMAGES + urlBuilt;
		}

		public async Task<SearchResponse<Media>> QuerySimilarImages(IEnumerable<GroupImagesAlike> similarImages, int pageLimit)
		{
			var response = new SearchResponse<Media>();
			var totalFound = new Media();
			try
			{
				{
					using var browser = _puppeteerClient.GetBrowser();
					using var page = await browser.NewPageAsync();

					foreach (var image in similarImages)
					{
						var totalSerpItems = new List<SerpItem>();
						var fullImageUrl = YANDEX_BASE_IMAGE_URL + image.Url + RPT;
						await page.GoToAsync(fullImageUrl);

						await Task.Delay(TimeSpan.FromSeconds(.55));
						if (page.Url.Contains("captcha"))
						{
							response.StatusCode = ResponseCode.CaptchaRequired;
							response.Message = "Yandex captcha needed";
							return response;
						}

						await page.ScrollBy(pageLimit);

						var htmlBody = await page.GetContentAsync();
						if (string.IsNullOrEmpty(htmlBody))
						{
							response.StatusCode = ResponseCode.NotFound;
							response.Message = "Html body empty";
							return response;
						}

						totalSerpItems.AddRange(ExtractSerpData(htmlBody));

						if (!totalSerpItems.Any())
						{
							response.StatusCode = ResponseCode.NotFound;
							response.Message = "no serp items returned";
							return response;
						}

						foreach (var item in totalSerpItems)
						{
							var mediaUrl = item?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url;
							if (mediaUrl == null) continue;
							if (mediaUrl.Contains(".gif")) continue;
							if (totalFound.Medias.Exists(_ => _.MediaId == mediaUrl)) continue;

							totalFound.Medias.Add(new MediaResponse
							{
								Topic = image.TopicGroup,
								MediaId = mediaUrl,
								MediaType = InstaMediaType.Image,
								MediaFrom = MediaFrom.Yandex,
								MediaUrl = new List<string>(new[] { mediaUrl }),
								Caption = item.Snippet?.Text,
								Title = item.Snippet?.Title,
								Domain = item.Snippet?.Domain
							});
						}
					}

					if (!totalFound.Medias.Any())
					{
						response.StatusCode = ResponseCode.NotFound;
						response.Message = "no similar images found";
						return response;
					}

					response.StatusCode = ResponseCode.Success;
					response.Result = totalFound;
					return response;
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				response.StatusCode = ResponseCode.InternalServerError;
				response.Message = err.Message;
				return response;
			}
		}
		public async Task<SearchResponse<Media>> QueryImages(YandexSearchQuery query, int pageLimit = 1)
		{
			var response = new SearchResponse<Media>();
			var totalFound = new Media();
			var url = QueryUrlBuilder(query);
			try
			{
				var totalSerpItems = new List<SerpItem>();

				{
					using var browser = _puppeteerClient.GetBrowser();
					using var page = await browser.NewPageAsync();

					await page.GoToAsync(url);
					await Task.Delay(TimeSpan.FromSeconds(.6));

					if (page.Url.Contains("captcha"))
					{
						response.StatusCode = ResponseCode.CaptchaRequired;
						response.Message = "Yandex captcha needed";
						return response;
					}

					await page.ScrollBy(pageLimit);

					var htmlBody = await page.GetContentAsync();
					if (string.IsNullOrEmpty(htmlBody))
					{
						response.StatusCode = ResponseCode.NotFound;
						response.Message = "Html body empty";
						return response;
					}
					totalSerpItems.AddRange(ExtractSerpData(htmlBody));
				}

				if (!totalSerpItems.Any())
				{
					response.StatusCode = ResponseCode.NotFound;
					response.Message = "no serp items returned";
					return response;
				}

				foreach (var item in totalSerpItems)
				{
					var mediaUrl = item?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault()?.Url;
					if (mediaUrl == null) continue;
					if (mediaUrl.Contains(".gif")) continue;
					if (totalFound.Medias.Exists(_ => _.MediaId == mediaUrl)) continue;

					totalFound.Medias.Add(new MediaResponse
					{
						Topic = query.OriginalTopic,
						MediaId = mediaUrl,
						MediaType = InstaMediaType.Image,
						MediaFrom = MediaFrom.Yandex,
						MediaUrl = new List<string>(new[] { mediaUrl }),
						Caption = item.Snippet?.Text,
						Title = item.Snippet?.Title,
						Domain = item.Snippet?.Domain
					});
				}

				response.StatusCode = ResponseCode.Success;
				response.Result = totalFound;
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
		private List<SerpItem> ExtractSerpData(string source)
		{
			var total = new List<SerpItem>();
			source = source.Replace("&quot;", "\"");
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
	}
}
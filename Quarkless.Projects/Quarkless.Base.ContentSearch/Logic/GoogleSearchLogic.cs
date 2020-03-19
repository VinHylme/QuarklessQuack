using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.ContentSearch.Models.Enums;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Models.Models;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.PuppeteerClient.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Search;
using Quarkless.Models.Common.Models.Topic;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.SearchResponse.Enums;

namespace Quarkless.Base.ContentSearch.Logic
{
	public class GoogleSearchLogic : IGoogleSearchLogic
	{
		private const string IMAGE_URL = "https://www.google.co.uk/search?";
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IPuppeteerClient _puppeteerClient;
		public GoogleSearchLogic(IPuppeteerClient puppeteerClient)
		{
			_puppeteerClient = puppeteerClient;
			_restSharpClient = new RestSharpClientManager.Logic.RestSharpClientManager();
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
				//_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
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

		public async Task<SearchResponse<Quarkless.Models.SearchResponse.Media>> SearchGoogleImages(CTopic topic, SearchGoogleImageRequestModel searchQuery)
		{
			var response = new SearchResponse<Quarkless.Models.SearchResponse.Media>();
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
				response.Result = new Quarkless.Models.SearchResponse.Media();
				var totalMedias = new List<MediaResponse>();
				string htmlBody;

				{
					using var browser = _puppeteerClient.GetBrowser();
					using var page = await browser.NewPageAsync();
					await page.GoToAsync(request);
					htmlBody = await page.GetContentAsync();
				}

				if (string.IsNullOrEmpty(htmlBody))
				{
					response.StatusCode = ResponseCode.NotFound;
					response.Message = "Html body returned empty";
					return response;
				}

				var json = new Regex("\\[\n*.*\"GRID_STATE0\"(?=[\\S\\s]{10,8000})[\\S\\s]*\\]$", RegexOptions.Multiline)
					.Matches(htmlBody).First().Value;

				var items = new Regex("\\[0,\"(.*?),\\[(.*?)\\]\n*,\\[(.*?)\\]", RegexOptions.Multiline)
					.Matches(json);

				totalMedias.AddRange(items.Select(_=>new MediaResponse
				{
					MediaId = _.Groups[1].Value.Replace("\"",""),
					MediaUrl = new List<string>(new []{ _.Groups[3].Value.Split(",")[0].Replace("\"","") }),
					MediaType = InstaMediaType.Image,
					MediaFrom = MediaFrom.Google,
					Topic = topic
				}));

				if (totalMedias.Any())
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

		public SearchResponse<Quarkless.Models.SearchResponse.Media> SearchSimilarImagesViaGoogle(IEnumerable<GroupImagesAlike> groupImages, int limit, int offset = 0)
		{
			return new SearchResponse<Quarkless.Models.SearchResponse.Media>
			{
				StatusCode = ResponseCode.InternalServerError
			};
		}

		public async Task<List<string>> GetSuggestions(string query)
		{
			var results = new List<string>();
			var request = QueryBuilder(new SearchGoogleImageRequestModel
			{
				Keyword = query
			});
			string htmlBody;

			{
				using var browser = _puppeteerClient.GetBrowser();
				using var page = await browser.NewPageAsync();
				await page.GoToAsync(request);
				htmlBody = await page.GetContentAsync();
			}

			if (string.IsNullOrEmpty(htmlBody))
			{
				return results;
			}

			var json = new Regex("\"online_chips\",\\[(?=[\\S\\s]{10,8000})[\\S\\s]*]$", RegexOptions.Multiline)
				.Matches(htmlBody).First().Value;

			var items = new Regex("(\\[\"(.*?)\")", RegexOptions.Multiline)
				.Matches(json);

			results.AddRange(items.Take(14).Skip(1).Select(item => item.Groups[2].Value));

			return results;
		}
	}
}

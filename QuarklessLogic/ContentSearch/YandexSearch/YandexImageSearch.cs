using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.RestSharpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuarklessLogic.ContentSearch.SeleniumClient;

namespace QuarklessLogic.ContentSearch.YandexSearch
{
	#region ResponseModels
	public struct MoreItem
	{
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("direction")]
		public string Direction { get; set; }
		[JsonProperty("visible")]
		public bool Visible { get; set; }
	}
	public class SearchItem
	{
		[JsonProperty("serpitem", NullValueHandling=NullValueHandling.Ignore)]
		public SerpItem SerpItem { get; set; }
	}
	public class SerpItem
	{
		[JsonProperty("reqid")]
		public string ReqID { get; set; }

		[JsonProperty("freshness")]
		public string Freshness { get; set; }

		[JsonProperty("preview")]
		public Preview[] Preview { get; set; }

		[JsonProperty("dups")]
		public Dup[] Dups { get; set; }

		[JsonProperty("thumb")]
		public Thumb Thumb { get; set; }

		[JsonProperty("snippet", NullValueHandling=NullValueHandling.Ignore)]
		public Snippet Snippet { get; set; }

		[JsonProperty("detail_url", NullValueHandling=NullValueHandling.Ignore)]
		public string Detail_Url { get; set; }

		[JsonProperty("img_href", NullValueHandling=NullValueHandling.Ignore)]
		public string Img_href { get; set; }

		[JsonProperty("useProxy", NullValueHandling=NullValueHandling.Ignore)]
		public bool UseProxy { get; set; }

		[JsonProperty("pos", NullValueHandling=NullValueHandling.Ignore)]
		public int Pos { get; set; }

		[JsonProperty("id", NullValueHandling=NullValueHandling.Ignore)]
		public string ID { get; set; }

		[JsonProperty("rimId", NullValueHandling=NullValueHandling.Ignore)]
		public string RimID { get; set; }

		[JsonProperty("docid", NullValueHandling=NullValueHandling.Ignore)]
		public string DocID { get; set; }

		[JsonProperty("greenUrlCounterPath", NullValueHandling=NullValueHandling.Ignore)]
		public string GreenUrlCounterPath { get; set; }

		[JsonProperty("counterPath", NullValueHandling=NullValueHandling.Ignore)]
		public string CounterPath { get; set; }
	}
	public class Thumb
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("size")]
		public Size Size { get; set; }

		[JsonProperty("microImg")]
		public string MicroImg { get; set; }
	}
	public class Size
	{
		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int height { get; set; }
	}
	public class Snippet
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("hasTitle")]
		public bool HasTitle { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("domain")]
		public string Domain { get; set; }

		[JsonProperty("redirUrl")]
		public string RedirUrl { get; set; }
	}
	public class Preview
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("fileSizeInBytes")]
		public int FileSizeInBytes { get; set; }

		[JsonProperty("w")]
		public int W { get; set; }

		[JsonProperty("h")]
		public int H { get; set; }
	}
	public class Dup
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("fileSizeInBytes")]
		public int FileSizeInBytes { get; set; }

		[JsonProperty("w")]
		public int W { get; set; }

		[JsonProperty("h")]
		public int H { get; set; }
	}
	public class RequestSettings
	{
		public Block[] blocks { get; set; }
		public Bmt bmt { get; set; }
		public Amt amt { get; set; }
	}
	public class Bmt
	{
		public string lb { get; set; }
	}
	public class Amt
	{
		public string las { get; set; }
	}
	public class Block
	{
		public string block { get; set; }
		public Params _params { get; set; }
		public int version { get; set; }
	}
	public class Params
	{
		public int initialPageNum { get; set; }
	}
	#endregion

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

		private readonly ISeleniumClient _seleniumClient;
		private readonly IRestSharpClientManager _restSharpClientManager;
		public YandexImageSearch(ISeleniumClient seleniumClient)
		{
			_restSharpClientManager = new RestSharpClientManager();

			_seleniumClient = seleniumClient;
			_seleniumClient.AddArguments(
				"headless",
				"--log-level=3",
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
					? $"http://{proxy.Address}:{proxy.Port}"
					: $"http://{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
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
				var result = _seleniumClient.Reader(url, limit);
				if (result?.Result != null) { 
					totalFound.Medias.AddRange(result.Result.Select(o => new MediaResponse
					{
						Topic = yandexSearchQuery.OriginalTopic,
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
					var result = _seleniumClient.YandexImageSearch(httpsYandexComImages, url.Url, "serp-item_pos_", limit);
					totalFound.Medias.AddRange(result.Where(s => !s.Contains(".gif")).Select(a => new MediaResponse
					{
						Topic = url.TopicGroup,
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
			return _seleniumClient.YandexSearchMe(imageUrl, numberOfPages, offset);
			#region REST VERSION (SPAM DETECTED A LOT)
//			RequestSettings requestSettings = new RequestSettings
//			{
//				blocks = new Block[]
//				{
//					new Block
//					{
//						block = "serp-controller",
//						version = 2,
//					},
//					new Block
//					{
//						block = "serp-list_infinite_yes",
//						_params = new Params
//						{
//							initialPageNum = 0
//						},
//						version = 2
//					},
//					new Block
//					{
//						block = "more_direction_next",
//						version = 2
//					},
//					new Block
//					{
//						block = "gallery__items:ajax",
//						version = 2
//					}
//				},
//				bmt = new Bmt
//				{
//					lb = "({yjI=52Fx"
//				},
//				amt = new Amt
//				{
//					las = "justifier-height=1;thumb-underlay=1;justifier-setheight=1;fitimages-height=1;justifier-fitincuts=1"
//				}
//			};
			
//			List<HttpHeader> headers = new List<HttpHeader>();
//			SearchResponse<List<SerpItem>> response = new SearchResponse<List<SerpItem>>();
//			List<SerpItem> totalCollected = new List<SerpItem>();
//			var imageData = "&yu={0}&p={1}&source=collections&cbir_id={2}&url={3}&uinfo={4}";
//			long randnum2 = (long)(SecureRandom.NextDouble() * 9000000000000) + 1000000000000;
//			var ajaxCallbackCompleteUrl = string.Format(ajaxCallBackUrl,randnum2.ToString());
//			var tostringserialised = JsonConvert.SerializeObject(requestSettings);

//			try
//			{
//				var cookiesResp = _restSharpClientManager.GetRequest(yandexImages, null);
//				IList<HttpHeader> localHeaders = new List<HttpHeader>();
//				if (cookiesResp.IsSuccessful) { 
//					_restSharpClientManager.AddCookies(cookiesResp.Cookies.Select(c => new System.Net.Cookie 
//					{
//						Comment = c.Comment,
//						CommentUri = c.CommentUri,
//						Discard = c.Discard,
//						Domain = c.Domain,
//						Expired = c.Expired,
//						Expires = c.Expires,
//						HttpOnly = c.HttpOnly,
//						Name = c.Name,
//						Path = c.Path,
//						Port = c.Port,
//						Secure = c.Secure,
//						Value = c.Value,
//						Version = c.Version
//					}));
//					localHeaders = cookiesResp.Headers.Select(h => new HttpHeader 
//					{
//						Name = h.Name,
//						Value = h.Value.ToString()
//					}).ToList();
//				}

//				var results = _restSharpClientManager.GetRequest(imageUrl, null, headers:localHeaders.Where(s=>!s.Value.Equals("chunked")));
//				if(results == null || (results.ResponseStatus == ResponseStatus.TimedOut || results.ResponseStatus == ResponseStatus.Error || results.ResponseStatus == ResponseStatus.Aborted))
//				{
//					response.StatusCode = ResponseCode.Timeout;
//					response.Message = "Timed out";
//					return response;
//				}
//				if (results.ResponseUri.AbsolutePath.Contains("captcha"))
//				{
//					//chance ip and retry
//					response.StatusCode = ResponseCode.CaptchaRequired;
//					response.Message = "Yandex captcha needed";
//					return response;
//				}
//				else
//				{
//					headers.AddRange(results.Headers.Select(s => new HttpHeader { Name = s.Name, Value = s.Value.ToString() }));
//					if (headers.Count > 3)
//					{
//						for (int currPage = 0; currPage < numberOfPages; )
//						{
//							//first items
//							var regexMatch = Regex.Matches(results.Content, "{\"serp-item\":.*?}}").Select(x => 
//							{ 
//								var newresults = x.Value.Replace("{\"serp-item\":","");
//								return newresults.Substring(0, newresults.Length - 1); 
//							});

//							if(regexMatch==null && regexMatch.Count()<=0)
//							{
//								break;
//							}
//							else { 
//								var convertToSerpObject = regexMatch.Select(s=> JsonConvert.DeserializeObject<SerpItem>(s)).ToList();
//								if(currPage>0)
//									convertToSerpObject.RemoveAt(0); //duplicates

//								totalCollected.AddRange(convertToSerpObject);
//								currPage++;
//								var moreavaliable = Regex.Match(results.Content, "{\"more\":.*?}}")
//									.Value.Replace("{\"more\":","");
//								if (string.IsNullOrEmpty(moreavaliable))
//								{
//									break;
//								}
//								else { 
//									MoreItem moreItem = JsonConvert.DeserializeObject<MoreItem>(moreavaliable.Substring(0,moreavaliable.Length - 1));
									
//									results = _restSharpClientManager.GetRequest(yandexUrl + moreItem.Url, null,headers:localHeaders.Where(s=>!s.Value.Equals("chunked")));
//								}
////								var nextpageUrl = imageUrl.Insert(imageUrl.IndexOf('?')+1,$"p={currPage}&");
////								results = _restSharpClientManager.GetRequest(nextpageUrl, null, headers:headers.Where(s=>!s.Value.Equals("chunked")));
//							}
//							#region api json version

//							/*
//							var cbirId = Regex.Match(results.Content, @"cbir_id=.*amp;").Value.Replace("&amp;", "").Replace("cbir_id=", "");
//							if (string.IsNullOrWhiteSpace(cbirId)||string.IsNullOrEmpty(cbirId))
//							{
//								imageData = "&yu={0}&p={1}&source=collections&url={2}&uinfo={3}";
//							}
							
//							var yoid = Regex.Match(Regex.Match(results.Content, @"((yandexuid)(.*?)[:](.*?)})").Value,@"\d+").Value;
//							var uinfo_complete = string.Format(uinfo_,Regex.Match(results.Content, @"(serpid)(.*?),").Value.Replace("\"","").Replace("serpid","").Replace(":","").Replace(",",""));
//							var imageDataPopulated = string.Format(imageData, yoid, currPage, cbirId, imageUrl, uinfo_complete);
//							var nextpageUrl = ajaxCallbackCompleteUrl + tostringserialised + imageDataPopulated;
							
//							var res = _restSharpClientManager.GetRequest(nextpageUrl, null, headers:headers.Where(s => !s.Value.Equals("chunked")));
//							if (!res.IsSuccessful || res.ResponseUri.AbsolutePath.Contains("captcha"))
//							{
//								response.StatusCode = ResponseCode.CaptchaRequired;
//								response.Result = totalCollected;
//							}
//							*/
//							//var regexMatch = Regex.Matches(res.Content, @"(serp-item)[\\](.*?)(counterPath)(.*?)(}})").Select(x => { var newres = x.Value.Replace("\\", "").Replace("serp-item" + "\":", ""); return newres.Substring(0, newres.Length - 1); });
//							//totalCollected  = regexMatch.Select(s => JsonConvert.DeserializeObject<SerpItem>(s)).ToList();
//							#endregion
//						}
//					}
//				}
//			}
//			catch (Exception ee)
//			{
//				response.Message = ee.Message;
//				response.StatusCode = ResponseCode.InternalServerError;
//				return response;
//			}

//			response.StatusCode = ResponseCode.Success;
//			response.Result = totalCollected;
//			return response;
			#endregion
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
		public Media Search(IEnumerable<GroupImagesAlike> similarImages, int limit)
		{
			//Media TotalFound = new Media();
			//foreach (var url in similarImages)
			//{
			//	if (url != null)
			//	{
			//		var fullurl_ = yandexBaseImageUrl + url.Url + rpt;
			//		try
			//		{
			//			var result = seleniumClient.Reader(fullurl_, "serp-item_pos_", limit);
			//			if (result == null) return null;
			//			TotalFound.Medias.AddRange(result.Where(s => !s.Contains(".gif")).Select(a => new MediaResponse
			//			{
			//				ProfileTopic = url.TopicGroup,
			//				MediaUrl = new List<string> { a },
			//				MediaFrom = MediaFrom.Yandex,
			//				Type = InstagramApiSharp.Classes.Models.InstaMediaType.Image
			//			}));
			//			Task.Delay(500);
			//		}
			//		catch (Exception ee)
			//		{
			//			Console.Write(ee.Message);
			//			TotalFound.errors++;
			//		}
			//	}
			//};
			//return TotalFound;
			return null;
		}
	}
}

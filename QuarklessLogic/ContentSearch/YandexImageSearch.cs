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

namespace QuarklessLogic.ContentSearch
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
		[JsonProperty("serpitem")]
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

		[JsonProperty("snippet")]
		public Snippet Snippet { get; set; }

		[JsonProperty("detail_url")]
		public string Detail_Url { get; set; }

		[JsonProperty("img_href")]
		public string Img_href { get; set; }

		[JsonProperty("useProxy")]
		public bool UseProxy { get; set; }

		[JsonProperty("pos")]
		public int Pos { get; set; }

		[JsonProperty("id")]
		public string ID { get; set; }

		[JsonProperty("rimId")]
		public string RimID { get; set; }

		[JsonProperty("docid")]
		public string DocID { get; set; }

		[JsonProperty("greenUrlCounterPath")]
		public string GreenUrlCounterPath { get; set; }

		[JsonProperty("counterPath")]
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

	public class YandexImageSearch
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

		SeleniumClient.SeleniumClient seleniumClient = new SeleniumClient.SeleniumClient();
		private readonly IRestSharpClientManager _restSharpClientManager;
		public YandexImageSearch(IRestSharpClientManager restSharpClientManager, ProxyModel proxy = null)
		{
			_restSharpClientManager = restSharpClientManager;
			if (proxy != null)
			{
				string proxyLine = string.Empty;
				if (string.IsNullOrEmpty(proxy.Username))
				{
					proxyLine = $"http://{proxy.Address}:{proxy.Port}";
				}
				else
				{
					proxyLine = $"http://{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
				}
				seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}
			seleniumClient.AddArguments(
				"headless",
				"--log-level=3",
				"--silent",
				"--disable-extensions",
				"test-type",
				"--ignore-certificate-errors",
				"no-sandbox");
		}
		private string BuildUrl(YandexSearchQuery yandexSearch)
		{
			string urlBuilt = string.Empty;
			if (!string.IsNullOrEmpty(yandexSearch.SearchQuery))
			{
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
			return null;
		}
		public SearchResponse<Media> SearchQueryREST(YandexSearchQuery yandexSearchQuery, int limit = 16)
		{
			SearchResponse<Media> response = new SearchResponse<Media>();
			Media TotalFound = new Media();
			try
			{
				var url = BuildUrl(yandexSearchQuery);
				var result = seleniumClient.Reader(url, limit);
				if (result.Result != null) { 
					TotalFound.Medias.AddRange(result.Result.Select(o => new MediaResponse
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
					Message = result.Message,
					Result = TotalFound,
					StatusCode = result.StatusCode
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
		public SearchResponse<Media> SearchSafeButSlow(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			Media TotalFound = new Media();
			SearchResponse<Media> response = new SearchResponse<Media>();

			ImagesLikeUrls.ToList().ForEach(url =>
			{
				if (url != null)
				{
					var fullurl_ = yandexImages;
					try
					{
						var result = seleniumClient.YandexImageSearch(fullurl_, url.Url, "serp-item_pos_", limit);
						TotalFound.Medias.AddRange(result.Where(s => !s.Contains(".gif")).Select(a => new MediaResponse
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
						TotalFound.errors++;
					}
				}
			});
			response.StatusCode = ResponseCode.Success;
			response.Result = TotalFound;
			return response;
		}

		public SearchResponse<List<SerpItem>> SearchRest(string imageUrl, int numberOfPages)
		{
			return seleniumClient.YandexSearchMe(imageUrl,numberOfPages);
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

		public SearchResponse<Media> SearchRelatedImagesREST(IEnumerable<GroupImagesAlike> imagesAlikes, int numberOfPages)
		{
			SearchResponse<Media> response = new SearchResponse<Media>();
			Media totalCollected = new Media();

			foreach (var url in imagesAlikes)
			{
				if (url != null)
				{
					var fullImageUrl = yandexBaseImageUrl + url.Url + rpt;
					try
					{
						var searchSerp = SearchRest(fullImageUrl, numberOfPages);
						if (searchSerp.StatusCode == ResponseCode.Success || (searchSerp.StatusCode == ResponseCode.CaptchaRequired && searchSerp?.Result?.Count > 0))
						{
							totalCollected.Medias.AddRange(searchSerp.Result.Select(o => new MediaResponse
							{
								MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image,
								MediaFrom = MediaFrom.Yandex,
								MediaUrl = new List<string> { o?.Preview?.OrderByDescending(s => s?.FileSizeInBytes).FirstOrDefault().Url },
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
			}
			response.Result = totalCollected;
			response.StatusCode = ResponseCode.Success;
			return response;
		}
		public Media Search(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			//Media TotalFound = new Media();
			//foreach (var url in ImagesLikeUrls)
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
			//				Topic = url.TopicGroup,
			//				MediaUrl = new List<string> { a },
			//				MediaFrom = MediaFrom.Yandex,
			//				MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image
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

using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContentSearcher
{
	public class YandexImageSearch
	{
		private const string yandexImages = @"https://yandex.com/images/";
		private const string yandexBaseImageUrl = @"https://yandex.com/images/search?url=";
		private const string rpt = @"&rpt=imagelike";

		SeleniumClient.SeleniumClient seleniumClient = new SeleniumClient.SeleniumClient();
		public YandexImageSearch()
		{
			seleniumClient.AddArguments(
				"headless",
				"--log-level=3",
				"--silent",
				"--disable-extensions",
				"test-type",
				"--ignore-certificate-errors",
				"no-sandbox");			
			//seleniumClient.Initialise();
		}
		private string BuildUrl(YandexSearchQuery yandexSearch)
		{
			string urlBuilt = string.Empty;
			if (!string.IsNullOrEmpty(yandexSearch.SearchQuery))
			{
				urlBuilt += $"search?text={HttpUtility.UrlEncode(yandexSearch.SearchQuery)}";
				if(yandexSearch.Orientation!= Orientation.Any)
				{
					urlBuilt+= "&iorient="+yandexSearch.Orientation.GetDescription();
				}
				if(yandexSearch.Type!= ImageType.Any)
				{
					urlBuilt+="&type="+yandexSearch.Type.GetDescription();
				}
				if(yandexSearch.Color != ColorType.Any)
				{
					urlBuilt+="&icolor="+yandexSearch.Color.GetDescription();
				}
				if(yandexSearch.Format != FormatType.Any)
				{
					urlBuilt+= "&itype="+yandexSearch.Format.GetDescription();
				}
				if(yandexSearch.Size != SizeType.None &&yandexSearch.SpecificSize == null)
				{
					urlBuilt+="&isize="+yandexSearch.Size.GetDescription();
				}
				if(yandexSearch.SpecificSize!=null && yandexSearch.Size == SizeType.None)
				{
					urlBuilt+="&isize=eq&iw="+ yandexSearch.SpecificSize.Value.Width +"&ih="+yandexSearch.SpecificSize.Value.Height;
				}
				return yandexImages + urlBuilt;
			}
			return null;
		}
		public Media SearchQueryREST(YandexSearchQuery yandexSearchQuery, int limit = 16)
		{
			Media TotalFound = new Media();
			try
			{
				var url = BuildUrl(yandexSearchQuery);
				var result = seleniumClient.Reader(url, "serp-item_pos_",limit);
				TotalFound.Medias.AddRange(result.Where(s => !s.Contains(".gif")).Select(a => new MediaResponse
				{
					Topic = yandexSearchQuery.SearchQuery,
					MediaUrl = new List<string> { a },
					MediaFrom = MediaFrom.Yandex,
					MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image
				}));
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				TotalFound.errors++;
			}
			return TotalFound;
		}
		public Media SearchSafeButSlow(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			Media TotalFound = new Media();
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
						TotalFound.errors++;
					}
				}
			});
			return TotalFound;
		}
		public Media Search(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			Media TotalFound = new Media();
			foreach(var url in ImagesLikeUrls)
			{
				if (url != null) { 
				var fullurl_ = yandexBaseImageUrl + url.Url + rpt;
					try
					{
						var result = seleniumClient.Reader(fullurl_, "serp-item_pos_", limit);
						if(result==null) return null;
						TotalFound.Medias.AddRange(result.Where(s=>!s.Contains(".gif")).Select(a=> new MediaResponse{
							Topic = url.TopicGroup,
							MediaUrl = new List<string>{ a },
							MediaFrom = MediaFrom.Yandex,
							MediaType = InstagramApiSharp.Classes.Models.InstaMediaType.Image
						}));
						Task.Delay(500);
					}
					catch (Exception ee)
					{
						Console.Write(ee.Message);
						TotalFound.errors++;
					}
				}
			};
			return TotalFound;
		}
	}
}

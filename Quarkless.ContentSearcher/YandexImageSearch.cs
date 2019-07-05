using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSearcher
{
	public class YandexImageSearch
	{
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
		}
		public Media Search(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			Media TotalFound = new Media();
			Parallel.ForEach(ImagesLikeUrls, url=>
			{
				if (url != null) { 
				var fullurl_ = yandexBaseImageUrl + url.Url + rpt;
					try
					{
						var result = seleniumClient.Reader(fullurl_, "serp-item_pos_", limit);
						TotalFound.Medias.AddRange(result.Where(s=>!s.Contains(".gif")).Select(a=> new MediaResponse{
							Topic = url.TopicGroup,
							MediaUrl = new List<string>{ a },
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
	}
}

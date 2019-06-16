﻿using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContentSearcher
{
	public class YandexImageSearch
	{
		private const string yandexBaseImageUrl = @"https://yandex.com/images/search?url=";
		private const string rpt = @"&rpt=imagelike";

		SeleniumClient.SeleniumClient seleniumClient = new SeleniumClient.SeleniumClient();
		public YandexImageSearch()
		{
			seleniumClient.AddArguments("headless");
		}
		public Media Search(IEnumerable<GroupImagesAlike> ImagesLikeUrls, int limit)
		{
			Media TotalFound = new Media();
			foreach (var url in ImagesLikeUrls)
			{
				var fullurl_ = yandexBaseImageUrl + url.Url + rpt;
				try
				{
					var result = seleniumClient.Reader(fullurl_, "serp-item_pos_", limit);
					MediaResponse imageResponse = new MediaResponse
					{
						Topic = url.TopicGroup,
						MediaUrl = result.ToList()
					};
					TotalFound.Medias.Add(imageResponse);
				}
				catch (Exception ee)
				{
					Console.Write(ee.Message);
					TotalFound.errors++;
				}
			}
			return TotalFound;
		}
	}
}

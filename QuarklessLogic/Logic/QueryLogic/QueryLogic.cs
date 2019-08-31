using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;
using QuarklessContexts.Models.QueryModels.Settings;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessRepositories.RedisRepository.SearchCache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq.Extensions;

namespace QuarklessLogic.Logic.QueryLogic
{
	public class QueryLogic : IQueryLogic
	{
		private readonly IRestSharpClientManager _restSharpClientManager;
		private readonly IContentSearcherHandler _contentSearcherHandler;
		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly ISearchingCache _searchingCache;
		private readonly IHashtagLogic _hashtagLogic;

		public QueryLogic(IRestSharpClientManager restSharpClientManager, IContentSearcherHandler contentSearcherHandler,
			ISearchingCache searchingCache, ITopicServicesLogic topicServicesLogic, IHashtagLogic hashtagLogic)
		{
			_hashtagLogic = hashtagLogic;
			_searchingCache = searchingCache;
			_restSharpClientManager = restSharpClientManager;
			_contentSearcherHandler = contentSearcherHandler;
			_topicServicesLogic = topicServicesLogic;
		}
		public object SearchPlaces(string query)
		{
			try
			{
				var results = _restSharpClientManager.GetRequest("https://maps.googleapis.com/maps/api/place/textsearch/json?query=" + query + "&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null);

				return results.Content;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public object AutoCompleteSearchPlaces(string query, int radius = 500)
		{
			try
			{
				return _restSharpClientManager.GetRequest($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={query}&radius={radius}&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null).Content;
			}
			catch(Exception e)
			{
				return null;
			}
		}
		
		public async Task<Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null, bool moreAccurate = false)
		{
			if(urls == null || (limit == 0)) return null;
			var requestData = urls as string[] ?? urls.ToArray();
			var strurl = requestData.Select(_ => new GroupImagesAlike { TopicGroup = "any", Url = _ }).ToList();
			var newLimit = limit;
			Media response;
			if (newLimit == -1)
			{
				var searchRequest = new SearchRequest
				{
					RequestData = requestData,
					Limit = newLimit,
					Offset = offset > 0 ? Math.Abs(newLimit - limit) : offset,
				};
				var cacheRes = await _searchingCache.GetSearchData(userId, searchRequest);
				if (cacheRes != null)
				{
					response = cacheRes.ResponseData;
					return response;
				}

				if (moreAccurate)
				{
					response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 6, 0).Result;
					response.Medias.Reverse();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _contentSearcherHandler.SearchSimilarImagesViaGoogle(strurl, 99);
					if (res.StatusCode == QuarklessContexts.Models.ResponseModels.ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 6, 0).Result;
						response.Medias.Reverse();
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
				}
			}
			else
			{
				if (offset == 1) offset = 0;
				if (offset > 0)
				{
					newLimit *= (offset);
				}

				var searchRequest = new SearchRequest
				{
					RequestData = requestData,
					Limit = newLimit,
					Offset = offset > 0 ? Math.Abs(newLimit - limit) : offset,
				};
				var cacheRes = await _searchingCache.GetSearchData(userId, searchRequest);
				if (cacheRes != null)
				{
					response = cacheRes.ResponseData;
					return response;
				}

				if (moreAccurate)
				{
					response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 1 + offset, offset).Result;
					response.Medias.Reverse();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _contentSearcherHandler.SearchSimilarImagesViaGoogle(strurl, newLimit,
						offset > 0 ? Math.Abs(newLimit - limit) : offset);
					if (res.StatusCode == QuarklessContexts.Models.ResponseModels.ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 1 + offset, offset).Result;
						response.Medias.Reverse();
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
				}
			}

			return response;
		}
		public async Task<ProfileConfiguration> GetProfileConfig()
		{
			return new ProfileConfiguration
			{
				Topics = (await _topicServicesLogic.GetAllTopicCategories()).DistinctBy(item=>item.CategoryName).OrderBy(item=>item.CategoryName),
				ColorsAllowed = Enum.GetValues(typeof(ColorType)).Cast<ColorType>().Select(v=>v.GetDescription()),
				ImageTypes = Enum.GetValues(typeof(ImageType)).Cast<ImageType>().Select(v=>v.GetDescription()),
				Orientations = Enum.GetValues(typeof(Orientation)).Cast<Orientation>().Select(v=>v.GetDescription()),
				SizeTypes = Enum.GetValues(typeof(SizeType)).Cast<SizeType>().Select(v=>v.GetDescription()),
				SearchTypes = Enum.GetValues(typeof(SearchType)).Cast<SearchType>().Select(v=>v.GetDescription()),
				Languages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(_=>!_.Name.Contains("-")).Distinct().ToDictionary(_ => _.Name, _ => _.EnglishName),
				CanUserEditProfile = true
			};
		}

		public async Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language,
			int limit = 1, int pickRate = 20)
		{
			var res = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic, language.ToUpper(), language.MapLanguages(),limit)).Shuffle().ToList();
			var clean = new Regex(@"[^\w\d]");
			if (res.Count <= 0) return null;
			var hashtags = new List<string>();
			while (hashtags.Count < pickRate) { 
				var chosenHashtags = new List<string>();
				foreach(var hashtagres in res)
				{
					if (string.IsNullOrEmpty(hashtagres.Language)) continue;
					var hlang = clean.Replace(hashtagres.Language.ToLower(),"");
					var langpicked = clean.Replace(language.MapLanguages().ToLower(),"");

					if(hlang == langpicked)
						chosenHashtags.AddRange(hashtagres.Hashtags);
				}
				if (chosenHashtags.Count <= 0) continue;
				var chosenHashtagsFiltered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
				var hashtagsFiltered = chosenHashtagsFiltered as string[] ?? chosenHashtagsFiltered.ToArray();
				if (!hashtagsFiltered.Any()) return null;
				hashtags.AddRange(hashtagsFiltered.Where(s => s.Length >= 3 && s.Length <= 30));
			}
			return hashtags.Take(pickRate);
		}

		public async Task<SubTopics> GetRelatedKeywords(string topicName)
		{
			var res = await _searchingCache.GetReleatedTopic(topicName);
			if (res != null && res.RelatedTopics.Count>0) return res;

			if (topicName.Contains('('))
			{
				var iterative = topicName.Split('(');
				var totalRes = new SubTopics {TopicName = topicName, RelatedTopics = new List<string>()};
				foreach (var topic in iterative)
				{
					var filtered = Regex.Replace(topic.ToLower(), "[^0-9a-zA-Z]+", "");
					var search = await _hashtagLogic.SearchHashtagAsync(filtered);
					if (!search.Succeeded)
					{
						var sRel = await _hashtagLogic.SearchReleatedHashtagAsync(topic, 1);
						if (!sRel.Succeeded) return null;
						totalRes.RelatedTopics.AddRange(sRel.Value.RelatedHashtags.Select(x=>x.Name));
					}
					else
					{
						totalRes.RelatedTopics.AddRange(search.Value.Select(x=>x.Name));
					}
				}
				if(totalRes.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(totalRes);
				return totalRes;
			}
			var cleanedTopic = Regex.Replace(topicName.ToLower(), "[^0-9a-zA-Z]+", "");
			var hashtagsRes = await _hashtagLogic.SearchHashtagAsync(cleanedTopic);
			if (!hashtagsRes.Succeeded)
			{
				var related = await _hashtagLogic.SearchReleatedHashtagAsync(cleanedTopic, 1);
				if (!related.Succeeded) return null;
				var subTopics = new SubTopics
				{
					TopicName = topicName,
					RelatedTopics = related.Value.RelatedHashtags.Select(s => s.Name).ToList()
				};
				if(subTopics.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(subTopics);
				return subTopics;

			}
			else
			{
				var subTopics = new SubTopics
				{
					TopicName = topicName,
					RelatedTopics = hashtagsRes.Value.Select(s=>s.Name).ToList()
				};
				if(subTopics.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(subTopics);
				return subTopics;
			}
		}
	}
}

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;

namespace QuarklessRepositories.RedisRepository.SearchCache
{
	public interface ISearchingCache
	{
		Task<List<TResult>> GetSearchData<TResult>(string id);
		Task AddSearchData<TResult>(string id, IEnumerable<TResult> data);

		Task<SearchRequest> GetSearchData(string userId, SearchRequest find, string instagramId = null, string profileId = null);
		Task StoreSearchData(string userId, SearchRequest search, string instagramId = null, string profileId = null);
		Task<SubTopics> GetRelatedTopic(string topic);
		Task StoreRelatedTopics(SubTopics subTopics);
	}
}
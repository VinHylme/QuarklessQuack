﻿using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.Hashtag.Models.Interfaces
{
	public interface IHashtagLogic
	{
		Task<IResult<bool>> FollowHashtagAsync(string tagName);
		Task<IResult<InstaHashtagSearch>> GetFollowingHashtagsInfoAsync(long userId);
		Task<IResult<InstaHashtag>> GetHashtagInfoAsync(string tagName);
		Task<IResult<InstaHashtagStory>> GetHashtagStoriesAsync(string tagName);
		Task<IResult<InstaSectionMedia>> GetRecentHashtagMediaListAsync(string tagName, PaginationParameters paginationParameters);
		Task<IResult<InstaHashtagSearch>> GetSuggestedHashtagsAsync();
		Task<IResult<InstaSectionMedia>> GetTopHashtagMediaListAsync(string topic, int limit);
		Task<IResult<InstaHashtagSearch>> SearchHashtagAsync(string query, IEnumerable<long> excludeList = null);
		Task<IResult<InstaSectionMedia>> SearchRelatedHashtagAsync(string query, int limit = 1);
		Task<IResult<bool>> UnFollowHashtagAsync(string tagName);
		Task AddHashtagsToRepository(IEnumerable<HashtagsModel> hashtags);
		Task<List<HashtagsModel>> GetHashtagsFromRepositoryByTopic(string topic, int limit = 1);
		Task<IEnumerable<HashtagsModel>> GetHashtags(int topicHashCode, int limit = -1, bool skip = true);
	}
}
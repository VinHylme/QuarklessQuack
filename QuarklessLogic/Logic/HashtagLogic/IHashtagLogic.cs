using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessLogic.Logic.HashtagLogic
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
		Task AddHashtagsToRepositoryAndCache(IEnumerable<HashtagsModel> hashtags);
		Task<IEnumerable<HashtagsModel>> GetHashtagsByTopicAndLanguage(string topic, string lang, string langmapped, int limit =1);
		Task UpdateAllMediasLanguagesToLower();

	}
}
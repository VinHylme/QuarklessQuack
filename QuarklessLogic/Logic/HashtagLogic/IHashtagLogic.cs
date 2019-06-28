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
		Task<IResult<bool>> FollowHashtagAsync(string tagname);
		Task<IResult<InstaHashtagSearch>> GetFollowingHashtagsInfoAsync(long userId);
		Task<IResult<InstaHashtag>> GetHashtagInfoAsync(string tagname);
		Task<IResult<InstaHashtagStory>> GetHashtagStoriesAsync(string tagname);
		Task<IResult<InstaSectionMedia>> GetRecentHashtagMediaListAsync(string tagname, PaginationParameters paginationParameters);
		Task<IResult<InstaHashtagSearch>> GetSuggestedHashtagsAsync();
		Task<IResult<InstaSectionMedia>> GetTopHashtagMediaListAsync(string topic, int limit);
		Task<IResult<InstaHashtagSearch>> SearchHashtagAsync(string query, IEnumerable<long> excludeList = null);
		Task<IResult<bool>> UnFollowHashtagAsync(string tagname);
		Task<IEnumerable<HashtagsModel>> GetHashtagsByTopic(string topic, int limit =1);
	}
}
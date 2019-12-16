using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository
{
	public interface IHashtagsRepository 
	{
		Task<bool> AddHashtags(IEnumerable<HashtagsModel> hashtags);
		Task<bool> RemoveHashtags(IEnumerable<string> hashtag_ids);
		Task<IEnumerable<HashtagsModel>> GetHashtags(IEnumerable<FilterDefinition<HashtagsModel>> searchRepository = null, int limit = -1);
		Task<IEnumerable<HashtagsModel>> GetHashtagsByTopic(string topic, int limit);
		Task<IEnumerable<HashtagsModel>> GetHashtags(string topic, string language = null, string mapedLang = null, int limit = -1);
		Task UpdateAllMediasLanguagesToLower();
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Library;

namespace QuarklessRepositories.RedisRepository.LibraryCache
{
	public interface ILibraryCache
	{
		Task AddSavedHashtags(string accountId, string instagramId, HashtagsLib hashtagsLib);
		Task AddSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib);
		Task AddSavedMedias(string accountId, string instagramId, MediasLib mediasLibs);
		Task DeleteSavedHashtags(string accountId, string instagramId,  HashtagsLib hashtagsLib);
		Task DeleteSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib);
		Task DeleteSavedMedias(string accountId, string instagramId,  MediasLib mediasLibs);
		Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId, string instagramId);
		Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId, string instagramId);
		Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId, string instagramId);
	}
}
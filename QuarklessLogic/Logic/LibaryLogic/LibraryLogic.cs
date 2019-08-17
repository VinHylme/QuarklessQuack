using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Library;
using QuarklessRepositories.RedisRepository.LibraryCache;

namespace QuarklessLogic.Logic.LibaryLogic
{
	public class LibraryLogic : ILibraryLogic
	{
		private readonly ILibraryCache _libraryCache;
		public LibraryLogic(ILibraryCache libraryCache) => _libraryCache = libraryCache;

		public Task AddSavedHashtags(string accountId, string instagramId, HashtagsLib hashtagsLib)
		{
			return _libraryCache.AddSavedHashtags(accountId, instagramId, hashtagsLib);
		}

		public Task AddSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib)
		{
			return _libraryCache.AddSavedCaptions(accountId, instagramId, captionsLib);
		}

		public Task AddSavedMedias(string accountId, string instagramId, MediasLib mediasLibs)
		{
			return _libraryCache.AddSavedMedias(accountId, instagramId, mediasLibs);
		}

		public Task DeleteSavedMedias(string accountId, string instagramId, MediasLib mediasLibs)
		{
			return _libraryCache.DeleteSavedMedias(accountId, instagramId, mediasLibs);
		}
		public Task DeleteSavedHashtags(string accountId, string instagramId, HashtagsLib hashtagsLib)
		{
			return _libraryCache.DeleteSavedHashtags(accountId, instagramId, hashtagsLib);
		}

		public Task DeleteSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib)
		{
			return _libraryCache.DeleteSavedCaptions(accountId, instagramId, captionsLib);
		}

		public Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId, string instagramId)
		{
			return _libraryCache.GetSavedMedias(accountId, instagramId);
		}

		public Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId, string instagramId)
		{
			return _libraryCache.GetSavedCaptions(accountId, instagramId);
		}

		public Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId, string instagramId)
		{
			return _libraryCache.GetSavedHashtags(accountId, instagramId);
		}
	}
}

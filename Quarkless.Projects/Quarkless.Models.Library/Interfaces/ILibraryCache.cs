using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Library.Interfaces
{
	public interface ILibraryCache
	{
		Task AddSavedMessages(MessagesLib messagesLib);
		Task AddSavedHashtags(HashtagsLib hashtagsLib);
		Task AddSavedCaptions(CaptionsLib captionsLib);
		Task AddSavedMedias(IEnumerable<MediasLib> mediasLibs);
		Task DeleteSavedMedias(MediasLib mediasLibs);
		Task DeleteSavedHashtags(HashtagsLib hashtagsLib);
		Task DeleteSavedCaptions(CaptionsLib captionsLib);
		Task DeleteSavedMessage(MessagesLib messagesLib);
		Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId);
		Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId);
		Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId);
		Task<IEnumerable<MessagesLib>> GetSavedMessages(string accountId);
	}
}
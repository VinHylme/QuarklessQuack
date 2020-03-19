using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.Library.Models.Interfaces
{
	public interface ILibraryLogic
	{
		Task<bool> AddSavedMessages(MessagesLib messagesLib);
		Task<bool> AddSavedHashtags(HashtagsLib hashtagsLib);
		Task<bool> AddSavedCaptions(CaptionsLib captionsLib);
		Task<bool> AddSavedMedias(IEnumerable<MediasLib> mediasLibs);
		Task<bool> DeleteSavedMedias(MediasLib mediasLibs);
		Task<bool> DeleteSavedHashtags(HashtagsLib hashtagsLib);
		Task<bool> DeleteSavedCaptions(CaptionsLib captionsLib);
		Task<bool> DeleteSavedMessage(MessagesLib messagesLib);
		Task<bool> UpdateSavedHashtags(HashtagsLib hashtagsLib);
		Task<bool>  UpdateSavedCaptions(CaptionsLib captionsLib);
		Task<bool> UpdateSavedMessage(MessagesLib messagesLib);
		Task<IEnumerable<MediasLib>> GetSavedMediasForUser(string instagramId);
		Task<IEnumerable<CaptionsLib>> GetSavedCaptionsForUser(string instagramId);
		Task<IEnumerable<HashtagsLib>> GetSavedHashtagsForUser(string instagramId);
		Task<IEnumerable<MessagesLib>> GetSavedMessagesForUser(string instagramId);
		Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId);
		Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId);
		Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId);
		Task<IEnumerable<MessagesLib>> GetSavedMessages(string accountId);
	}
}
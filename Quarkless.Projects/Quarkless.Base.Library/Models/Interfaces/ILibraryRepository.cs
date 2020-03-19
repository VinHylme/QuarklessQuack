using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.Library.Models.Interfaces
{
	public interface ILibraryRepository
	{
		Task<bool> AddMedias(IEnumerable<MediasLib> medias);
		Task<IEnumerable<MediasLib>> GetAllMediasForUser(string instagramAccountId);
		Task<IEnumerable<MediasLib>> GetAllMediasForAccountHolder(string accountId);
		Task<bool> AddCaption(CaptionsLib caption);
		Task<bool> DeleteMedia(MediasLib media);
		Task<IEnumerable<CaptionsLib>> GetAllCaptionForUser(string instagramAccountId);
		Task<IEnumerable<CaptionsLib>> GetAllCaptionsForAccountHolder(string accountId);
		Task<bool> UpdateCaption(CaptionsLib caption);
		Task<bool> DeleteCaption(CaptionsLib caption);
		Task<bool> AddHashtags(HashtagsLib hashtags);
		Task<IEnumerable<HashtagsLib>> GetAllHashtagsForUser(string instagramAccountId);
		Task<IEnumerable<HashtagsLib>> GetAllHashtagsForAccountHolder(string accountId);
		Task<bool> UpdateHashtags(HashtagsLib hashtags);
		Task<bool> DeleteHashtags(HashtagsLib hashtags);
		Task<bool> AddMessage(MessagesLib messagesLib);
		Task<IEnumerable<MessagesLib>> GetAllMessagesForUser(string instagramAccountId);
		Task<IEnumerable<MessagesLib>> GetAllMessagesForAccountHolder(string accountId);
		Task<bool> UpdateMessage(MessagesLib message);
		Task<bool> DeleteMessage(MessagesLib message);
	}
}
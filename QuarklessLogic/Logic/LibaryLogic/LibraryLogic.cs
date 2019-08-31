using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Library;
using QuarklessRepositories.Repository.LibraryRepository;

namespace QuarklessLogic.Logic.LibaryLogic
{
	public class LibraryLogic : ILibraryLogic
	{
		private readonly ILibraryRepository _libraryRepository;
		public LibraryLogic(ILibraryRepository libraryRepository) => _libraryRepository = libraryRepository;

		public async Task<bool> AddSavedMessages(MessagesLib messagesLib) =>
			await _libraryRepository.AddMessage(messagesLib);
		public async Task<bool> AddSavedHashtags(HashtagsLib hashtagsLib) =>
			await _libraryRepository.AddHashtags(hashtagsLib);
		public async Task<bool> AddSavedCaptions(CaptionsLib captionsLib) =>
			await _libraryRepository.AddCaption(captionsLib);
		public async Task<bool> AddSavedMedias(IEnumerable<MediasLib> mediasLibs) =>
			await _libraryRepository.AddMedias(mediasLibs);
		

		public async Task<bool> DeleteSavedMedias(MediasLib mediasLibs)=>
			await _libraryRepository.DeleteMedia(mediasLibs);
		public async Task<bool> DeleteSavedHashtags(HashtagsLib hashtagsLib) =>
			await _libraryRepository.DeleteHashtags(hashtagsLib);
		public async Task<bool> DeleteSavedCaptions(CaptionsLib captionsLib) =>
			await _libraryRepository.DeleteCaption(captionsLib);
		public async Task<bool> DeleteSavedMessage(MessagesLib messagesLib) =>
			await _libraryRepository.DeleteMessage(messagesLib);

		public async Task<bool> UpdateSavedHashtags(HashtagsLib hashtagsLib) =>
			await _libraryRepository.UpdateHashtags(hashtagsLib);
		public async Task<bool>  UpdateSavedCaptions(CaptionsLib captionsLib) =>
			await _libraryRepository.UpdateCaption(captionsLib);
		public async Task<bool> UpdateSavedMessage(MessagesLib messagesLib) =>
			await _libraryRepository.UpdateMessage(messagesLib);

		public async Task<IEnumerable<MediasLib>> GetSavedMediasForUser(string instagramId)=>
			await _libraryRepository.GetAllMediasForUser(instagramId);
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptionsForUser(string instagramId)=>
			await _libraryRepository.GetAllCaptionForUser(instagramId);
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtagsForUser(string instagramId)=>
			await _libraryRepository.GetAllHashtagsForUser(instagramId);
		public async Task<IEnumerable<MessagesLib>> GetSavedMessagesForUser(string instagramId)=>
			await _libraryRepository.GetAllMessagesForUser(instagramId);

		public async Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId)=>
			await _libraryRepository.GetAllMediasForAccountHolder(accountId);
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId)=>
			await _libraryRepository.GetAllCaptionsForAccountHolder(accountId);
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId)=>
			await _libraryRepository.GetAllHashtagsForAccountHolder(accountId);
		public async Task<IEnumerable<MessagesLib>> GetSavedMessages(string accountId)=>
			await _libraryRepository.GetAllMessagesForAccountHolder(accountId);

	}
}

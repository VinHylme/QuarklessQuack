using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.Library.Models;
using Quarkless.Base.Library.Models.Interfaces;

namespace Quarkless.Base.Library.Logic
{
	public class LibraryLogic : ILibraryLogic
	{
		private readonly ILibraryRepository _libraryRepository;
		private readonly ILibraryCache _libraryCache;

		public LibraryLogic(ILibraryRepository libraryRepository, ILibraryCache libraryCache)
		{
			_libraryRepository = libraryRepository;
			_libraryCache = libraryCache;
		}

		public async Task<bool> AddSavedMessages(MessagesLib messagesLib)
		{
			if (messagesLib == null) return false;
			var res  = await _libraryRepository.AddMessage(messagesLib);
			if (res)
				await _libraryCache.AddSavedMessages(messagesLib);
			return res;
		}
		public async Task<bool> AddSavedHashtags(HashtagsLib hashtagsLib)
		{
			if (hashtagsLib == null) return false;
			var res = await _libraryRepository.AddHashtags(hashtagsLib);
			if (res)
				await _libraryCache.AddSavedHashtags(hashtagsLib);
			return res;
		}
		public async Task<bool> AddSavedCaptions(CaptionsLib captionsLib)
		{
			if (captionsLib == null) return false;
			var res = await _libraryRepository.AddCaption(captionsLib);
			if (res)
				await _libraryCache.AddSavedCaptions(captionsLib);
			return res;
		}
		public async Task<bool> AddSavedMedias(IEnumerable<MediasLib> mediasLibs)
		{
			if (!mediasLibs.Any()) return false;
			var res = await _libraryRepository.AddMedias(mediasLibs);
			if (res)
				await _libraryCache.AddSavedMedias(mediasLibs);
			return res;
		}

		public async Task<bool> DeleteSavedMedias(MediasLib mediasLibs)
		{
			var res = await _libraryRepository.DeleteMedia(mediasLibs);
			if (res)
				await _libraryCache.DeleteSavedMedias(mediasLibs);
			return res;
		}
		public async Task<bool> DeleteSavedHashtags(HashtagsLib hashtagsLib)
		{
			var res = await _libraryRepository.DeleteHashtags(hashtagsLib);
			if (res)
				await _libraryCache.DeleteSavedHashtags(hashtagsLib);
			return res;
		}
		public async Task<bool> DeleteSavedCaptions(CaptionsLib captionsLib)
		{
			var res = await _libraryRepository.DeleteCaption(captionsLib);
			if (res)
				await _libraryCache.DeleteSavedCaptions(captionsLib);
			return res;
		}
		public async Task<bool> DeleteSavedMessage(MessagesLib messagesLib)
		{
			var res = await _libraryRepository.DeleteMessage(messagesLib);
			if (res)
				await _libraryCache.DeleteSavedMessage(messagesLib);
			return res;
		}

		public async Task<bool> UpdateSavedHashtags(HashtagsLib hashtagsLib)
		{
			var res = await _libraryRepository.UpdateHashtags(hashtagsLib);
			if (!res) return res;
			var oldRes = (await _libraryCache.GetSavedHashtags(hashtagsLib.AccountId)).Where(x=>x._id == hashtagsLib._id);
			if (oldRes.Any())
			{
				await _libraryCache.DeleteSavedHashtags(oldRes.FirstOrDefault());
				await _libraryCache.AddSavedHashtags(hashtagsLib);
			}
			return res;
		}
		public async Task<bool> UpdateSavedCaptions(CaptionsLib captionsLib)
		{
			var res = await _libraryRepository.UpdateCaption(captionsLib);
			if (!res) return res;

			var oldRes = (await _libraryCache.GetSavedCaptions(captionsLib.AccountId)).Where(x=>x._id == captionsLib._id);
			if (oldRes.Any())
			{
				await _libraryCache.DeleteSavedCaptions(oldRes.FirstOrDefault());
				await _libraryCache.AddSavedCaptions(captionsLib);
			}

			return res;
		}
		public async Task<bool> UpdateSavedMessage(MessagesLib messagesLib)
		{
			var res = await _libraryRepository.UpdateMessage(messagesLib);
			if (!res) return res;

			var oldRes = (await _libraryCache.GetSavedMessages(messagesLib.AccountId)).Where(x=>x._id == messagesLib._id);
			if (oldRes.Any())
			{
				await _libraryCache.DeleteSavedMessage(oldRes.FirstOrDefault());
				await _libraryCache.AddSavedMessages(messagesLib);
			}

			return res;
		}

		public async Task<IEnumerable<MediasLib>> GetSavedMediasForUser(string instagramId)=>
			await _libraryRepository.GetAllMediasForUser(instagramId);
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptionsForUser(string instagramId)=>
			await _libraryRepository.GetAllCaptionForUser(instagramId);
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtagsForUser(string instagramId)=>
			await _libraryRepository.GetAllHashtagsForUser(instagramId);
		public async Task<IEnumerable<MessagesLib>> GetSavedMessagesForUser(string instagramId)=>
			await _libraryRepository.GetAllMessagesForUser(instagramId);

		public async Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId)
		{
			var fromCache = await _libraryCache.GetSavedMedias(accountId);
			if (fromCache.Any())
				return fromCache;

			var fromDB = await _libraryRepository.GetAllMediasForAccountHolder(accountId);
			if (fromDB.Any())
				await _libraryCache.AddSavedMedias(fromDB);

			return fromDB;
		}
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId)
		{
			var fromCache = await _libraryCache.GetSavedCaptions(accountId);
			if (fromCache.Any())
				return fromCache;

			var fromDB = await _libraryRepository.GetAllCaptionsForAccountHolder(accountId);
			if (!fromDB.Any()) return fromDB;
			foreach (var captionsLib in fromDB)
			{
				await _libraryCache.AddSavedCaptions(captionsLib);
				await Task.Delay(150);
			}
			return fromDB;
		}
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId)
		{
			var fromCache = await _libraryCache.GetSavedHashtags(accountId);
			if (fromCache.Any())
				return fromCache;

			var fromDB = await _libraryRepository.GetAllHashtagsForAccountHolder(accountId);
			if (!fromDB.Any()) return fromDB;
			foreach (var hashtagsLib in fromDB)
			{
				await _libraryCache.AddSavedHashtags(hashtagsLib);
				await Task.Delay(150);
			}
			return fromDB;
		}
		public async Task<IEnumerable<MessagesLib>> GetSavedMessages(string accountId)
		{
			var fromCache = await _libraryCache.GetSavedMessages(accountId);
			if (fromCache.Any())
				return fromCache;

			var fromDB = await _libraryRepository.GetAllMessagesForAccountHolder(accountId);
			if (!fromDB.Any()) return fromDB;
			foreach (var messagesLib in fromDB)
			{
				await _libraryCache.AddSavedMessages(messagesLib);
				await Task.Delay(150);
			}
			return fromDB;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models.Library;
using QuarklessRepositories.RepositoryClientManager;

namespace QuarklessRepositories.Repository.LibraryRepository
{
	public class LibraryRepository : ILibraryRepository
	{
		private readonly IRepositoryContext _context;
		public LibraryRepository(IRepositoryContext context) => _context = context;

		public async Task<bool> AddMedias(IEnumerable<MediasLib> medias)
		{
			try
			{
				await _context.MediaLibrary.InsertManyAsync(medias);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<IEnumerable<MediasLib>> GetAllMediasForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(instagramAccountId)) return null;
			try
			{
				var filter = Builders<MediasLib>.Filter.Eq(_ => _.InstagramAccountId, instagramAccountId);
				var results = await _context.MediaLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<IEnumerable<MediasLib>> GetAllMediasForAccountHolder(string accountId)
		{
			if (string.IsNullOrEmpty(accountId)) return null;
			try
			{
				var filter = Builders<MediasLib>.Filter.Eq(_ => _.AccountId, accountId);
				var results = await _context.MediaLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<bool> DeleteMedia(MediasLib media)
		{
			if (media == null) return false;
			if (string.IsNullOrEmpty(media._id)) return false;
			try
			{
				var res = await _context.MediaLibrary.DeleteOneAsync(_ => _._id == media._id);
				return res.IsAcknowledged && res.DeletedCount > 0;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}

		public async Task<bool> AddCaption(CaptionsLib caption)
		{
			try
			{
				await _context.CaptionLibrary.InsertOneAsync(caption);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<IEnumerable<CaptionsLib>> GetAllCaptionForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(instagramAccountId)) return null;
			try
			{
				var filter = Builders<CaptionsLib>.Filter.Eq(_ => _.InstagramAccountId, instagramAccountId);
				var results = await _context.CaptionLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<IEnumerable<CaptionsLib>> GetAllCaptionsForAccountHolder(string accountId)
		{
			if (string.IsNullOrEmpty(accountId)) return null;
			try
			{
				var filter = Builders<CaptionsLib>.Filter.Eq(_ => _.AccountId, accountId);
				var results = await _context.CaptionLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<bool> UpdateCaption(CaptionsLib caption)
		{
			if (caption == null) return false;
			if (string.IsNullOrEmpty(caption._id)) return false;
			try
			{
				var builder = Builders<CaptionsLib>.Filter;
				var filter = builder.Eq(x => x._id, caption._id);
				await _context.CaptionLibrary.ReplaceOneAsync(filter, caption);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<bool> DeleteCaption(CaptionsLib caption)
		{
			if (caption == null) return false;
			if (string.IsNullOrEmpty(caption._id)) return false;
			try
			{
				var res = await _context.CaptionLibrary.DeleteOneAsync(_ => _._id == caption._id);
				return res.IsAcknowledged && res.DeletedCount > 0;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}

		public async Task<bool> AddHashtags(HashtagsLib hashtags)
		{
			try
			{
				await _context.HashtagLibrary.InsertOneAsync(hashtags);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<IEnumerable<HashtagsLib>> GetAllHashtagsForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(instagramAccountId)) return null;
			try
			{
				var filter = Builders<HashtagsLib>.Filter.Eq(_ => _.InstagramAccountId, instagramAccountId);
				var results = await _context.HashtagLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<IEnumerable<HashtagsLib>> GetAllHashtagsForAccountHolder(string accountId)
		{
			if (string.IsNullOrEmpty(accountId)) return null;
			try
			{
				var filter = Builders<HashtagsLib>.Filter.Eq(_ => _.AccountId, accountId);
				var results = await _context.HashtagLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<bool> UpdateHashtags(HashtagsLib hashtags)
		{
			if (hashtags == null) return false;
			if (string.IsNullOrEmpty(hashtags._id)) return false;
			try
			{
				var builder = Builders<HashtagsLib>.Filter;
				var filter = builder.Eq(x => x._id, hashtags._id);
				await _context.HashtagLibrary.ReplaceOneAsync(filter, hashtags);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<bool> DeleteHashtags(HashtagsLib hashtags)
		{
			if (hashtags == null) return false;
			if (string.IsNullOrEmpty(hashtags._id)) return false;
			try
			{
				var res = await _context.HashtagLibrary.DeleteOneAsync(_ => _._id == hashtags._id);
				return res.IsAcknowledged && res.DeletedCount > 0;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}

		public async Task<bool> AddMessage(MessagesLib messagesLib)
		{
			try
			{
				await _context.MessagesLibrary.InsertOneAsync(messagesLib);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<IEnumerable<MessagesLib>> GetAllMessagesForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(instagramAccountId)) return null;
			try
			{
				var filter = Builders<MessagesLib>.Filter.Eq(_ => _.InstagramAccountId, instagramAccountId);
				var results = await _context.MessagesLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<IEnumerable<MessagesLib>> GetAllMessagesForAccountHolder(string accountId)
		{
			if (string.IsNullOrEmpty(accountId)) return null;
			try
			{
				var filter = Builders<MessagesLib>.Filter.Eq(_ => _.AccountId, accountId);
				var results = await _context.MessagesLibrary.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<bool> UpdateMessage(MessagesLib message)
		{
			if (message == null) return false;
			if (string.IsNullOrEmpty(message._id)) return false;
			try
			{
				var builder = Builders<MessagesLib>.Filter;
				var filter = builder.Eq(x => x._id, message._id);
				await _context.MessagesLibrary.ReplaceOneAsync(filter, message);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<bool> DeleteMessage(MessagesLib message)
		{
			if (message == null) return false;
			if (string.IsNullOrEmpty(message._id)) return false;
			try
			{
				var res = await _context.MessagesLibrary.DeleteOneAsync(_ => _._id == message._id);
				return res.IsAcknowledged && res.DeletedCount > 0;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
	}
}

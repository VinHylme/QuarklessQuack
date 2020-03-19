using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Quarkless.Base.Library.Models;
using Quarkless.Base.Library.Models.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Base.Library.Repository
{
	public class LibraryRepository : ILibraryRepository
	{
		private readonly IMongoCollection<MediasLib> _mediasCtx;
		private readonly IMongoCollection<HashtagsLib> _hashtagsCtx;
		private readonly IMongoCollection<CaptionsLib> _captionsCtx;
		private readonly IMongoCollection<MessagesLib> _messagesCtx;

		private const string MEDIAS_COLLECTION_NAME = "UsersMediasLibrary";
		private const string HASHTAGS_COLLECTION_NAME = "UsersHashtagsLibrary"; 
		private const string CAPTIONS_COLLECTION_NAME = "UsersCaptionsLibrary";
		private const string MESSAGES_COLLECTION_NAME = "UsersMessagesLibrary";

		public LibraryRepository(IMongoClientContext context)
		{
			_mediasCtx = context.AccountDatabase.GetCollection<MediasLib>(MEDIAS_COLLECTION_NAME);
			_hashtagsCtx = context.AccountDatabase.GetCollection<HashtagsLib>(HASHTAGS_COLLECTION_NAME);
			_captionsCtx = context.AccountDatabase.GetCollection<CaptionsLib>(CAPTIONS_COLLECTION_NAME);
			_messagesCtx = context.AccountDatabase.GetCollection<MessagesLib>(MESSAGES_COLLECTION_NAME);
		}

		public async Task<bool> AddMedias(IEnumerable<MediasLib> medias)
		{
			try
			{
				await _mediasCtx.InsertManyAsync(medias);
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
				var results = await _mediasCtx.FindAsync(filter);
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
				var results = await _mediasCtx.FindAsync(filter);
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
				var res = await _mediasCtx.DeleteOneAsync(_ => _._id == media._id);
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
				await _captionsCtx.InsertOneAsync(caption);
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
				var results = await _captionsCtx.FindAsync(filter);
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
				var results = await _captionsCtx.FindAsync(filter);
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
				await _captionsCtx.ReplaceOneAsync(filter, caption);
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
				var res = await _captionsCtx.DeleteOneAsync(_ => _._id == caption._id);
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
				await _hashtagsCtx.InsertOneAsync(hashtags);
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
				var results = await _hashtagsCtx.FindAsync(filter);
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
				var results = await _hashtagsCtx.FindAsync(filter);
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
				await _hashtagsCtx.ReplaceOneAsync(filter, hashtags);
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
				var res = await _hashtagsCtx.DeleteOneAsync(_ => _._id == hashtags._id);
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
				await _messagesCtx.InsertOneAsync(messagesLib);
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
				var results = await _messagesCtx.FindAsync(filter);
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
				var results = await _messagesCtx.FindAsync(filter);
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
				await _messagesCtx.ReplaceOneAsync(filter, message);
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
				var res = await _messagesCtx.DeleteOneAsync(_ => _._id == message._id);
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

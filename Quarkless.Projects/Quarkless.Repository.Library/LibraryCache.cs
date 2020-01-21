using Quarkless.Models.Library;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Repository.RedisContext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Extensions;
using Quarkless.Repository.RedisContext.Models;
using StackExchange.Redis;

namespace Quarkless.Repository.Library
{
	public class LibraryCache : ILibraryCache
	{
		private readonly IRedisClient _redisClient;
		public LibraryCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}

		public async Task AddSavedMessages(MessagesLib messagesLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(MessagesLib)}:{messagesLib.AccountId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					messagesLib.ToJsonString(), TimeSpan.FromDays(360));
			}, messagesLib.AccountId, messagesLib.InstagramAccountId);
		}
		public async Task AddSavedHashtags(HashtagsLib hashtagsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(HashtagsLib)}:{hashtagsLib.AccountId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					hashtagsLib.ToJsonString(), TimeSpan.FromDays(360));
			}, hashtagsLib.AccountId, hashtagsLib.InstagramAccountId);
		}
		public async Task AddSavedCaptions(CaptionsLib captionsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(CaptionsLib)}:{captionsLib.AccountId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					captionsLib.ToJsonString(), TimeSpan.FromDays(360));
			}, captionsLib.AccountId, captionsLib.InstagramAccountId);
		}
		public async Task AddSavedMedias(IEnumerable<MediasLib> mediasLibs)
		{
			foreach (var mediasLib in mediasLibs)
			{
				await WithExceptionLogAsync(async () =>
				{
					RedisKey key = $"{nameof(MediasLib)}:{mediasLib.AccountId}";
					await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
						mediasLib.ToJsonString(), TimeSpan.FromDays(360));
				}, mediasLib.AccountId, mediasLib.InstagramAccountId);
				await Task.Delay(SecureRandom.Next(250,500));
			}
		}

		public async Task DeleteSavedMedias(MediasLib mediasLibs)
		{
			await WithExceptionLogAsync(async() =>
			{
				RedisKey key = $"{nameof(MediasLib)}:{mediasLibs.AccountId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, mediasLibs.ToJsonString());
			}, mediasLibs.AccountId, mediasLibs.InstagramAccountId);
		}
		public async Task DeleteSavedHashtags(HashtagsLib hashtagsLib)
		{
			await WithExceptionLogAsync(async() =>
			{
				RedisKey key = $"{nameof(HashtagsLib)}:{hashtagsLib.AccountId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, hashtagsLib.ToJsonString());
			}, hashtagsLib.AccountId, hashtagsLib.InstagramAccountId);
		}
		public async Task DeleteSavedCaptions(CaptionsLib captionsLib)
		{
			await WithExceptionLogAsync(async() =>
			{
				RedisKey key = $"{nameof(CaptionsLib)}:{captionsLib.AccountId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, captionsLib.ToJsonString());
			}, captionsLib.AccountId, captionsLib.InstagramAccountId);
		}
		public async Task DeleteSavedMessage(MessagesLib messagesLib)
		{
			await WithExceptionLogAsync(async() =>
			{
				RedisKey key = $"{nameof(MessagesLib)}:{messagesLib.AccountId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, messagesLib.ToJsonString());
			}, messagesLib.AccountId, messagesLib.InstagramAccountId);
		}

		public async Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId)
		{
			IEnumerable<MediasLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(MediasLib)}:{accountId}";
				results = await _redisClient.GetMembers<MediasLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, "");

			return results;
		}
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId)
		{
			IEnumerable<CaptionsLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(CaptionsLib)}:{accountId}";
				results = await _redisClient.GetMembers<CaptionsLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, "");

			return results;
		}
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId)
		{
			IEnumerable<HashtagsLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(HashtagsLib)}:{accountId}";
				results = await _redisClient.GetMembers<HashtagsLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, "");

			return results;
		}
		public async Task<IEnumerable<MessagesLib>> GetSavedMessages(string accountId)
		{
			IEnumerable<MessagesLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{nameof(MessagesLib)}:{accountId}";
				results = await _redisClient.GetMembers<MessagesLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, "");

			return results;
		}

		private Task WithExceptionLogAsync(Func<Task> actionAsync, string accountId, string instagramId)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}: for user {accountId} of account {instagramId}");
				throw;
			}
		}
	}
}

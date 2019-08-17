using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuarklessContexts.Models.Library;
using QuarklessRepositories.RedisRepository.RedisClient;
using StackExchange.Redis;

namespace QuarklessRepositories.RedisRepository.LibraryCache
{
	public class LibraryCache : ILibraryCache
	{
		private readonly IRedisClient _redisClient;
		public LibraryCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}

		public async Task AddSavedHashtags(string accountId, string instagramId, HashtagsLib hashtagsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					JsonConvert.SerializeObject(hashtagsLib), TimeSpan.FromDays(960));
			}, accountId, instagramId);
		}
		public async Task AddSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					JsonConvert.SerializeObject(captionsLib), TimeSpan.FromDays(960));
			}, accountId, instagramId);
		}
		public async Task AddSavedMedias(string accountId, string instagramId, MediasLib mediasLibs)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetAdd(key, RedisKeys.HashtagGrowKeys.UserLibrary,
					JsonConvert.SerializeObject(mediasLibs), TimeSpan.FromDays(960));
			}, accountId, instagramId);
		}

		public async Task DeleteSavedHashtags(string accountId, string instagramId, HashtagsLib hashtagsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, JsonConvert.SerializeObject(hashtagsLib));
			}, accountId, instagramId);
		}

		public async Task DeleteSavedCaptions(string accountId, string instagramId, CaptionsLib captionsLib)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, JsonConvert.SerializeObject(captionsLib));
			}, accountId, instagramId);		}

		public async Task DeleteSavedMedias(string accountId, string instagramId, MediasLib mediasLibs)
		{
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				await _redisClient.SetRemove(key, RedisKeys.HashtagGrowKeys.UserLibrary, JsonConvert.SerializeObject(mediasLibs));
			}, accountId, instagramId);		}

		public async Task<IEnumerable<MediasLib>> GetSavedMedias(string accountId, string instagramId)
		{
			IEnumerable<MediasLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				results = await _redisClient.GetMembers<MediasLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, instagramId);

			return results;
		}
		public async Task<IEnumerable<CaptionsLib>> GetSavedCaptions(string accountId, string instagramId)
		{
			IEnumerable<CaptionsLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				results = await _redisClient.GetMembers<CaptionsLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, instagramId);

			return results;
		}
		public async Task<IEnumerable<HashtagsLib>> GetSavedHashtags(string accountId, string instagramId)
		{
			IEnumerable<HashtagsLib> results = null;
			await WithExceptionLogAsync(async () =>
			{
				RedisKey key = $"{accountId}:{instagramId}";
				results = await _redisClient.GetMembers<HashtagsLib>(key, RedisKeys.HashtagGrowKeys.UserLibrary);

			}, accountId, instagramId);

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

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using static QuarklessRepositories.RedisRepository.RedisClient.RedisKeys;

namespace QuarklessRepositories.RedisRepository.RedisClient
{
	public static class KeyFormater
	{
		public static string FormatKey(string userId, HashtagGrowKeys hashtagGrowKey)
		{
			string formatTemplate = $"HashtagGrow:{hashtagGrowKey.ToString()}:({userId}:{hashtagGrowKey.ToString()})";
			return formatTemplate;
		}

	}
	public class RedisClient : IRedisClient
	{
		private ConnectionMultiplexer _redis;
		private readonly TimeSpan _defaultKeyExpiry;
		public RedisClient(IOptions<RedisOptions> options)
		{
			if (options != null)
			{
				try
				{
					ConfigurationOptions configuration = ConfigurationOptions.Parse(options.Value.ConnectionString);
					_redis = ConnectionMultiplexer.Connect(configuration);
					_redis.ConnectionFailed += _redis_ConnectionFailed;
					_redis.ErrorMessage += _redis_ErrorMessage;
					_redis.ConnectionRestored += _redis_ConnectionRestored;
					_defaultKeyExpiry = options.Value.DefaultKeyExpiry;
				}
				catch(Exception ee)
				{
					return;
				}
			}
		}
		#region Settings
		public void CloseConnection()
		{
			_redis.ErrorMessage -= _redis_ErrorMessage;
			_redis.ConnectionFailed -= _redis_ConnectionFailed;
			_redis.ConnectionRestored -= _redis_ConnectionRestored;
			_redis.Close(true);
			_redis = null;
		}
		public void Dispose()
		{
			if (_redis != null)
			{
				CloseConnection();
			}
		}
		private void _redis_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
		{
			Console.WriteLine($"Connection has been restored, Connection type: {e.ConnectionType}, Endpoint: {e.EndPoint}, Message: {e.Exception}");
		}
		private void _redis_ErrorMessage(object sender, RedisErrorEventArgs e)
		{
			Console.WriteLine($"Connection detected Error, Endpoint: {e.EndPoint}, Message: {e.Message}");
		}
		private void _redis_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
		{
			Console.WriteLine($"Connection has failed, Connection type: {e.ConnectionType}, Endpoint: {e.EndPoint}, Message: {e.Exception}");
		}
		#endregion

		#region Getters / Setters
		public IEnumerable<RedisKey> GetKeys(int limit)
		{
			try
			{
				var res = _redis.GetServer(_redis.GetEndPoints().First()).Keys(pageSize:limit);
				return res;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<bool> KeyExists(string userId, HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase().KeyExistsAsync(redisKey);
			}, userId, redisKey.ToString());
			return exists;
		}
		public async Task SetKeyExpiryDate(string userId, HashtagGrowKeys hashtagGrowKey, TimeSpan expires)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().KeyExpireAsync(redisKey, expires);
			}, userId, redisKey.ToString());
		}
		public async Task SetAdd(string userId, HashtagGrowKeys hashtagGrowKey, string value, TimeSpan? expires = null)
		{
			expires = expires != null && expires <= _defaultKeyExpiry ? expires : _defaultKeyExpiry;
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);

			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().SetAddAsync(redisKey, value);
				await _redis.GetDatabase().KeyExpireAsync(redisKey, expires);
			}, userId, redisKey.ToString());
		}
		public async Task<bool> SetMemberExists(string userId, HashtagGrowKeys hashtagGrowKey, string value)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase().SetContainsAsync(redisKey, value);
			}, userId, redisKey.ToString());

			return exists;
		}
		public async Task<IEnumerable<T>> GetMembers<T>(HashtagGrowKeys hashtagGrowKeys)
		{
			var members = new List<T>();
			RedisKey redisKey = hashtagGrowKeys.ToString();
			await WithExceptionLogAsync(async () =>
			{
				var redisMembers = await _redis.GetDatabase().SetMembersAsync(redisKey);
				members.AddRange(redisMembers.Select(m=>JsonConvert.DeserializeObject<T>(((string)m))));
			},"",redisKey.ToString());

			return members;
		}
		public async Task<IEnumerable<T>> GetMembers<T>(string userId, HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			var members = new List<T>();

			await WithExceptionLogAsync(async () =>
			{
				var redisMembers = await _redis.GetDatabase().SetMembersAsync(redisKey);
				members.AddRange(redisMembers.Select(m => JsonConvert.DeserializeObject<T>(((string) m))));
			}, userId, redisKey.ToString());

			return members;
		}
		public async Task SetRemove(string userId, HashtagGrowKeys hashtagGrowKey, string value)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().SetRemoveAsync(redisKey, value);
			}, userId, redisKey.ToString());
		}
		public async Task<T> GetHashField<T>(string userId, HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			T castResult = default(T);

			await WithExceptionLogAsync(async () =>
			{
				RedisValue result = await _redis.GetDatabase().HashGetAsync(redisKey, field);
				castResult = (T)Convert.ChangeType(result, typeof(T));
			}, userId, redisKey.ToString());

			return castResult;
		}
		public async Task SetHashField(string userId, HashtagGrowKeys hashtagGrowKey, string field, string value)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);

			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().HashSetAsync(redisKey, field, value);

			}, userId, redisKey.ToString());
		}
		public async Task RemoveHashField(string userId, HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);

			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().HashDeleteAsync(redisKey, field);

			}, userId, redisKey.ToString());
		}
		public async Task<bool> ExistsHashField(string userId, HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase().HashExistsAsync(redisKey, field);

			}, userId, redisKey.ToString());

			return exists;
		}
		public async Task<string> StringGet(string userId,HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			string val = string.Empty;
			await WithExceptionLogAsync(async () =>
			{
				val = await _redis.GetDatabase().StringGetAsync(redisKey);
			},userId,redisKey.ToString());
			return val;
		}
		public async Task StringSet(string userId, HashtagGrowKeys hashtagGrowKey, string value,TimeSpan? expiry = null, When when = When.Always)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().StringSetAsync(redisKey,value,expiry,when);
			}, userId, redisKey.ToString());
		}
		public async Task DeleteKey(string userId, HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormater.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase().KeyDeleteAsync(redisKey);
			}, userId, redisKey.ToString());
		}

		#endregion
		private Task WithExceptionLogAsync(Func<Task> actionAsync, string userId, string key)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message} User: {userId}, RedisKey: {key}");
				throw;
			}
		}
	}
}

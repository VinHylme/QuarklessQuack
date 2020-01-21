using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quarkless.Repository.RedisContext.Extensions;
using Quarkless.Repository.RedisContext.Models;
using StackExchange.Redis;

namespace Quarkless.Repository.RedisContext
{
	public class RedisClient : IRedisClient
	{
		private ConnectionMultiplexer _redis;
		private int _dBNumber { get; set; }
		private readonly TimeSpan _defaultKeyExpiry;
		public RedisClient(IOptions<RedisOptions> options)
		{
			if (options == null) return;
			try
			{
				var configuration = ConfigurationOptions.Parse(options.Value.ConnectionString);
				_redis = ConnectionMultiplexer.Connect(configuration);
				_redis.ConnectionFailed += _redis_ConnectionFailed;
				_redis.ErrorMessage += _redis_ErrorMessage;
				_redis.ConnectionRestored += _redis_ConnectionRestored;
				_defaultKeyExpiry = options.Value.DefaultKeyExpiry;
				_dBNumber = options.Value.DatabaseNumber;
			}
			catch
			{
				// ignored
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
		
		public IRedisClient Database(int newdb)
		{
			_dBNumber = newdb; 
			return this;
		}
		#region Getters / Setters
		public IEnumerable<RedisKey> GetKeys(int limit)
		{
			try
			{
				var res = _redis.GetServer(_redis.GetEndPoints().First()).Keys(_dBNumber, pageSize:limit);
				return res;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public RedisKey GetKey(RedisKey redisKey, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			try
			{
				var res = _redis.GetServer(_redis.GetEndPoints().First()).Keys(_dBNumber).FirstOrDefault(x=> x == KeyFormatter.FormatKey(redisKey, hashtagGrowKey));
				return res;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return string.Empty;
			}
		}
		public async Task<bool> KeyExists(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase(_dBNumber).KeyExistsAsync(redisKey);
			}, userId, redisKey.ToString());
			return exists;
		}
		public async Task SetKeyExpiryDate(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, TimeSpan expires)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).KeyExpireAsync(redisKey, expires);
			}, userId, redisKey.ToString());
		}
		public async Task SetAdd(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value, TimeSpan? expires = null)
		{
			expires = expires != null && expires <= _defaultKeyExpiry ? expires : _defaultKeyExpiry;
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).SetAddAsync(redisKey, value);
				await _redis.GetDatabase(_dBNumber).KeyExpireAsync(redisKey, expires);
			}, userId, redisKey.ToString());
		}
		public async Task SetAdd(string hashtagGrowKey, string value, TimeSpan? expires = null)
		{
			expires = expires != null && expires <= _defaultKeyExpiry ? expires : _defaultKeyExpiry;
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).SetAddAsync(hashtagGrowKey, value);
				await _redis.GetDatabase(_dBNumber).KeyExpireAsync(hashtagGrowKey, expires);
			}, "", hashtagGrowKey);
		}
		public async Task<bool> SetMemberExists(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase(_dBNumber).SetContainsAsync(redisKey, value);
			}, userId, redisKey.ToString());

			return exists;
		}
		public async Task<IEnumerable<T>> GetMembersFromKey<T>(string key, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			var members = new List<T>();
			await WithExceptionLogAsync(async () =>
			{

				var redisMembers = await _redis.GetDatabase(_dBNumber).SetMembersAsync(KeyFormatter.FormatKeyVal(key,hashtagGrowKey));
				members.AddRange(redisMembers.Select(m=>JsonConvert.DeserializeObject<T>(((string)m))));
			},"",key.ToString());

			return members;
		}
		public async Task<IEnumerable<T>> GetMembers<T>(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			var members = new List<T>();

			await WithExceptionLogAsync(async () =>
			{
				var redisMembers = await _redis.GetDatabase(_dBNumber).SetMembersAsync(redisKey);
				members.AddRange(redisMembers.Select(m => JsonConvert.DeserializeObject<T>(((string) m))));
			}, userId, redisKey.ToString());

			return members;
		}
		public async Task<IEnumerable<T>> GetMembers<T>(RedisKey redisKey)
		{
			var members = new List<T>();

			await WithExceptionLogAsync(async () =>
			{
				var redisMembers = await _redis.GetDatabase(_dBNumber).SetMembersAsync(redisKey);
				members.AddRange(redisMembers.Select(m => JsonConvert.DeserializeObject<T>(((string)m))));
			},"", redisKey.ToString());

			return members;
		}
		public async Task SetRemove(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).SetRemoveAsync(redisKey, value);
			}, userId, redisKey.ToString());
		}
		public async Task<T> GetHashField<T>(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			T castResult = default(T);

			await WithExceptionLogAsync(async () =>
			{
				RedisValue result = await _redis.GetDatabase(_dBNumber).HashGetAsync(redisKey, field);
				castResult = (T)Convert.ChangeType(result, typeof(T));
			}, userId, redisKey.ToString());

			return castResult;
		}
		public HashEntry[] GetHash(RedisKey key)
		{
			return _redis.GetDatabase(_dBNumber).HashGetAll(key);
		}

		public async Task SetHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field, string value)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);

			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).HashSetAsync(redisKey, field, value);

			}, userId, redisKey.ToString());
		}
		public async Task RemoveHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);

			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).HashDeleteAsync(redisKey, field);

			}, userId, redisKey.ToString());
		}
		public async Task<bool> ExistsHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			bool exists = false;
			await WithExceptionLogAsync(async () =>
			{
				exists = await _redis.GetDatabase(_dBNumber).HashExistsAsync(redisKey, field);

			}, userId, redisKey.ToString());

			return exists;
		}
		
		public async Task<string> StringGet(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			string val = string.Empty;
			await WithExceptionLogAsync(async () =>
			{
				val = await _redis.GetDatabase(_dBNumber).StringGetAsync(redisKey);
			},userId,redisKey.ToString());
			return val;
		}
		public async Task<string> StringGet(RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = hashtagGrowKey.ToString();
			string val = string.Empty;
			await WithExceptionLogAsync(async () =>
			{
				val = await _redis.GetDatabase(_dBNumber).StringGetAsync(redisKey);
			},"ALL", redisKey.ToString());
			return val;
		}
		public async Task StringSet(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value,TimeSpan? expiry = null, When when = When.Always)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).StringSetAsync(redisKey,value,expiry,when);
			}, userId, redisKey.ToString());
		}
		public async Task StringSet(RedisKeys.HashtagGrowKeys hashtagGrowKey, string value, TimeSpan? expiry = null, When when = When.Always)
		{
			RedisKey key = hashtagGrowKey.ToString();
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).StringSetAsync(key,value,expiry,when);
			}, "", key.ToString());
		}
		public async Task DeleteKey(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			RedisKey redisKey = KeyFormatter.FormatKey(userId, hashtagGrowKey);
			await WithExceptionLogAsync(async () =>
			{
				await _redis.GetDatabase(_dBNumber).KeyDeleteAsync(redisKey);
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

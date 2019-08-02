using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.RedisClient
{
	public interface IRedisClient
	{
		IRedisClient Database (int newdb);
		IEnumerable<RedisKey> GetKeys(int limit);
		void CloseConnection();
		Task DeleteKey(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey);
		void Dispose();
		Task<bool> ExistsHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field);
		Task<T> GetHashField<T>(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field);
		Task<IEnumerable<T>> GetMembers<T>(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey);
		Task<IEnumerable<T>> GetMembersFromKey<T>(string key, RedisKeys.HashtagGrowKeys hashtagGrowKey);
		Task<bool> KeyExists(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey);
		Task RemoveHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field);
		Task SetAdd(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value, TimeSpan? expires = null);
		Task SetHashField(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string field, string value);
		Task SetKeyExpiryDate(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, TimeSpan expires);
		Task<bool> SetMemberExists(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value);
		Task<string> StringGet(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey);
		Task StringSet(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value, TimeSpan? expiry = null, When when = When.Always);
		Task SetRemove(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey, string value);
	}
}
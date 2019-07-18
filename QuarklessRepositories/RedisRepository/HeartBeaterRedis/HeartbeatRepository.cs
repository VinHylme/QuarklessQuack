using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessRepositories.RedisRepository.RedisClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.HeartBeaterRedis
{
	public class HeartbeatRepository : IHeartbeatRepository
	{
		private readonly IRedisClient _redis;
		public HeartbeatRepository(IRedisClient redis)
		{
			_redis = redis;
		}
		public async Task<bool> MetaDataContains(MetaDataType metaDataType, string topic, string data, string userId)
		{
			bool contains = false;
			await WithExceptionLogAsync(async () =>
			{
				if (userId != null)
				{
					contains = await _redis.SetMemberExists(metaDataType.ToString() + ":" + topic + ":" + userId, RedisKeys.HashtagGrowKeys.MetaData, data);
				}
				else
				{
					contains = await _redis.SetMemberExists(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData, data);
				}
			});
			return contains;
		}
		public async Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId)
		{
			await WithExceptionLogAsync(async () => {
				if (userId != null)
				{
					await _redis.SetAdd(metaDataType.ToString() + ":" + topic + ":"+ userId, RedisKeys.HashtagGrowKeys.MetaData, JsonConvert.SerializeObject(data), TimeSpan.FromMinutes(45));
				}
				else { 
					await _redis.SetAdd(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData, JsonConvert.SerializeObject(data), TimeSpan.FromMinutes(45));
				}
			});
		}
		public async Task DeleteMetaDataFromSet<T>(MetaDataType metaDataType, string topic, __Meta__<T> dataToDelete, string userId = null)
		{
			await WithExceptionLogAsync(async () => {
				if (userId == null)
				{
					await _redis.SetRemove(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData, JsonConvert.SerializeObject(dataToDelete));
				}
				else
				{
					await _redis.SetRemove(metaDataType.ToString() + ":" + topic + ":" + userId, RedisKeys.HashtagGrowKeys.MetaData, JsonConvert.SerializeObject(dataToDelete));
				}
			});
		}
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic,string userId =null)
		{
			await WithExceptionLogAsync(async () => {
				if (userId == null) { 
					await _redis.DeleteKey(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData);
				}else
				{
					await _redis.DeleteKey(metaDataType.ToString()+":"+topic+":"+userId,RedisKeys.HashtagGrowKeys.MetaData);
				}
			});
		}
		public async Task<IEnumerable<__Meta__<T>>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId)
		{
			IEnumerable<__Meta__<T>> response = null;
			await WithExceptionLogAsync(async () => {
				if (userId != null)
				{
					response = await _redis.GetMembers<__Meta__<T>>(metaDataType.ToString() + ":" + topic +":"+userId, RedisKeys.HashtagGrowKeys.MetaData);
				}
				else { 
					response = await _redis.GetMembers<__Meta__<T>>(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData);
				}
			});
			return response;
		}
		public async Task<__Meta__<Media>> GetMediaMetaData(MetaDataType metaDataType, string topic)
		{
			__Meta__<Media> media = null;
			await WithExceptionLogAsync(async () => {
				media = JsonConvert.DeserializeObject<__Meta__<Media>>(await _redis.StringGet(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData));
			});

			return media;
		}
		public async Task<__Meta__<List<UserResponse<string>>>> GetUserFromLikers(MetaDataType metaDataType, string topic)
		{
			__Meta__<List<UserResponse<string>>> response = null;
			await WithExceptionLogAsync(async() => {
				response = JsonConvert.DeserializeObject<__Meta__<List<UserResponse<string>>>>(await _redis.StringGet(metaDataType.ToString() + ":" + topic, RedisKeys.HashtagGrowKeys.MetaData));
			});
			return response;
		}
		private Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;

namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public class HeartbeatLogic : IHeartbeatLogic
	{
		private readonly IHeartbeatRepository _heartbeatRepository;
		public HeartbeatLogic(IHeartbeatRepository heartbeatRepository) => _heartbeatRepository = heartbeatRepository;
		public async Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			await _heartbeatRepository.AddMetaData(metaDataType,topic,data,userId);
		}
		public async Task UpdateMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			await _heartbeatRepository.DeleteMetaDataFromSet(metaDataType,topic,data,userId);
			await _heartbeatRepository.AddMetaData(metaDataType, topic, data, userId);
		}
		public async Task<__Meta__<Media>?> GetMediaMetaData(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetMediaMetaData(metaDataType,topic);
		}
		public async Task<IEnumerable<__Meta__<T>?>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId = null)
		{
			return await _heartbeatRepository.GetMetaData<T>(metaDataType,topic, userId);
		}
		public async Task<__Meta__<List<UserResponse<string>>>?> GetUserFromLikers(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetUserFromLikers(metaDataType, topic);
		}
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null)
		{
			await _heartbeatRepository.RefreshMetaData(metaDataType,topic,userId);
		}
	}
}

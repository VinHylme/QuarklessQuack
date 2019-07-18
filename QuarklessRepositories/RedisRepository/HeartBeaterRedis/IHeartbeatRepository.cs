using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.HeartBeaterRedis
{
	public interface IHeartbeatRepository
	{
		Task<bool> MetaDataContains(MetaDataType metaDataType, string topic, string data, string userId);
		Task RefreshMetaData(MetaDataType metaDataType, string topic,string userId = null);
		Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null);
		Task<IEnumerable<__Meta__<T>>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId = null);
		Task<__Meta__<Media>> GetMediaMetaData(MetaDataType metaDataType, string topic);
		Task DeleteMetaDataFromSet<T>(MetaDataType metaDataType, string topic, __Meta__<T> dataToDelete, string userId = null);
		Task<__Meta__<List<UserResponse<string>>>> GetUserFromLikers(MetaDataType metaDataType, string topic);
	}
}

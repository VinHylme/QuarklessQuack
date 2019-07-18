using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public interface IHeartbeatLogic
	{
		Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null, ProxyModel proxy = null);
		Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null);
		Task UpdateMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null);
		Task<IEnumerable<__Meta__<T>>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId = null);
		Task<__Meta__<Media>> GetMediaMetaData(MetaDataType metaDataType, string topic);
		Task<__Meta__<List<UserResponse<string>>>> GetUserFromLikers(MetaDataType metaDataType, string topic);
	}
}

using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;

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
		void PopulateCaption(Media item, string topic);
		void PopulateComments(List<UserResponse<InstaComment>> comments, string topic);
		Task CleanCorpus();
	}
}

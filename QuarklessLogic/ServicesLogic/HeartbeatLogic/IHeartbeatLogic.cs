using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.ServicesModels.FetcherModels;

namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public interface IHeartbeatLogic
	{
		Task RefreshMetaData(MetaDataFetchRequest request);
		Task AddMetaData<TInput>(MetaDataCommitRequest<TInput> request);
		Task UpdateMetaData<TInput>(MetaDataCommitRequest<TInput> request);
		Task<IEnumerable<Meta<TInput>>> GetMetaData<TInput>(MetaDataFetchRequest request);

		Task<IEnumerable<TResults>> GetTempMetaData<TResults>(MetaDataTempType type);
		Task DeleteMetaDataTemp(MetaDataTempType type);
	}
}

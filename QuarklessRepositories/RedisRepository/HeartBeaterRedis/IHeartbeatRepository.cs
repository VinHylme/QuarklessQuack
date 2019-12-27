using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.ServicesModels.FetcherModels;

namespace QuarklessRepositories.RedisRepository.HeartBeaterRedis
{
	public interface IHeartbeatRepository
	{
		Task DeleteMetaDataTemp(MetaDataTempType type);
		Task<List<TResult>> GetTempMetaData<TResult>(MetaDataTempType type);

		Task<bool> MetaDataContains(MetaDataContainsRequest request);
		Task RefreshMetaData(MetaDataFetchRequest request);
		Task AddMetaData<TInput>(MetaDataCommitRequest<TInput> request);
		Task<List<Meta<TResult>>> GetMetaData<TResult>(MetaDataFetchRequest request);
		Task DeleteMetaDataFromSet<TInput>(MetaDataCommitRequest<TInput> request);
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Heartbeat.Models.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Heartbeat.Models.Interfaces
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
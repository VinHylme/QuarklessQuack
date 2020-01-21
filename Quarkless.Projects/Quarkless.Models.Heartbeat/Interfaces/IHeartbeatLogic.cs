using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Heartbeat.Enums;

namespace Quarkless.Models.Heartbeat.Interfaces
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
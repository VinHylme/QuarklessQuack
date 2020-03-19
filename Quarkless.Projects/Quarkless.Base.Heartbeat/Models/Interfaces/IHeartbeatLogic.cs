using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Heartbeat.Models.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Heartbeat.Models.Interfaces
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
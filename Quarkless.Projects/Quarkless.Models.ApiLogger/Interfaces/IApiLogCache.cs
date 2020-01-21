using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.ApiLogger.Interfaces
{
	public interface IApiLogCache
	{
		Task LogData(ApiLogMetaData apiLogMeta);
		Task<IEnumerable<ApiLogMetaData>> GetAllLogData();
	}
}
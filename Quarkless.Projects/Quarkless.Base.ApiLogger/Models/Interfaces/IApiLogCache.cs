using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.ApiLogger.Models.Interfaces
{
	public interface IApiLogCache
	{
		Task LogData(ApiLogMetaData apiLogMeta);
		Task<IEnumerable<ApiLogMetaData>> GetAllLogData();
	}
}
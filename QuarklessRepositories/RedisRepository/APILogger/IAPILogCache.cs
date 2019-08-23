using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.APILogger;

namespace QuarklessRepositories.RedisRepository.APILogger
{
	public interface IAPILogCache
	{
		Task LogData(ApiLogMetaData apiLogMeta);
		Task<IEnumerable<ApiLogMetaData>> GetAllLogData();
	}
}
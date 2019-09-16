using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.LoggerStoring
{
	public interface ILoggerStore
	{
		Task<IEnumerable<LoggerModel>> GetLog(string accountId, string instagramAccountId = null);
		Task<IEnumerable<LoggerModel>> GetLogBySection(string section, SeverityLevel severityLevel = SeverityLevel.All);
		Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All);
		Task Log(LoggerModel loggerModel);
	}
}
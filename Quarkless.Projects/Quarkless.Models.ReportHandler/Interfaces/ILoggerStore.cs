using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.ReportHandler.Enums;

namespace Quarkless.Models.ReportHandler.Interfaces
{
	public interface ILoggerStore
	{
		Task<IEnumerable<LoggerModel>> GetLog(string accountId, string instagramAccountId = null);
		Task<IEnumerable<LoggerModel>> GetLogBySection(string section, SeverityLevel severityLevel = SeverityLevel.All);
		Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All);
		Task Log(LoggerModel loggerModel);
	}
}

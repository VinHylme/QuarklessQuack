using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.ReportHandler.Models.Enums;

namespace Quarkless.Base.ReportHandler.Models.Interfaces
{
	public interface ILoggerStore
	{
		Task<IEnumerable<LoggerModel>> GetLog(string accountId, string instagramAccountId = null);
		Task<IEnumerable<LoggerModel>> GetLogBySection(string section, SeverityLevel severityLevel = SeverityLevel.All);
		Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All);
		Task Log(LoggerModel loggerModel);
	}
}

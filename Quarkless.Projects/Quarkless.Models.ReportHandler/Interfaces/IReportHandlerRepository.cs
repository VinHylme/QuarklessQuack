using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.ReportHandler.Interfaces
{
	public interface IReportHandlerRepository
	{
		Task InsertReport(LoggerModel logger);
		Task<IEnumerable<LoggerModel>> GetAllLogs();
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.ReportHandler.Models.Interfaces
{
	public interface IReportHandlerRepository
	{
		Task InsertReport(LoggerModel logger);
		Task<IEnumerable<LoggerModel>> GetAllLogs();
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.ReportHandler.Models.Enums;

namespace Quarkless.Base.ReportHandler.Models.Interfaces
{
	public interface IReportHandler
	{
		void SetupReportHandler(string type = "", string accountId = "", string username = "");
		Task MakeReport(Exception errors);
		Task MakeReport(ResultInfo result);
		Task MakeReport(string result);
		Task Log(LoggerModel logger);
		Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All);
	}
}

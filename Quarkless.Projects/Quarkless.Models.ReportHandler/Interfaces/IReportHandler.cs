using InstagramApiSharp.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.ReportHandler.Enums;

namespace Quarkless.Models.ReportHandler.Interfaces
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

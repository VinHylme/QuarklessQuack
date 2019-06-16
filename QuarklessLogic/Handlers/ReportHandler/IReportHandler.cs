using InstagramApiSharp.Classes;
using System;

namespace QuarklessLogic.Handlers.ReportHandler
{
	public interface IReportHandler
	{
		void SetupReportHandler(string type="", string accountId ="", string username="");
		void MakeReport(Exception errors);
		void MakeReport(ResultInfo result);
		void MakeReport(string result);
	}
}

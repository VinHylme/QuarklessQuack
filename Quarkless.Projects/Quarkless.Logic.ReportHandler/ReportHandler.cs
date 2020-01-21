using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.ReportHandler;
using Quarkless.Models.ReportHandler.Enums;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Logic.ReportHandler
{
	public class ReportHandler : IReportHandler
	{
		private string _username;
		private string _accountId;
		private string _type;
		private readonly ILoggerStore _loggerStore;
		private readonly IReportHandlerRepository _reportHandlerRepository;
		public ReportHandler(IReportHandlerRepository reportHandlerRepository, ILoggerStore loggerStore)
		{
			_loggerStore = loggerStore;
			_reportHandlerRepository = reportHandlerRepository;
		}

		public void SetupReportHandler(string type = "", string accountId = "", string username = "")
		{
			this._username = username;
			this._accountId = accountId;
			this._type = type;
		}

		public async Task Log(LoggerModel logger)
		{
			await _loggerStore.Log(logger);
		}
		public async Task<IEnumerable<LoggerModel>> GetLogs(SeverityLevel severityLevel = SeverityLevel.All)
		{
			return await _loggerStore.GetLogs(severityLevel);
		}
		public async Task<IEnumerable<LoggerModel>> GetLogBySection(string section, SeverityLevel severityLevel = SeverityLevel.All)
		{
			return await _loggerStore.GetLogBySection(section, severityLevel);
		}
		
		public async Task MakeReport(Exception errors)
		{
			var formatException = errors.Message + "::" + errors.Source + "::" + errors.StackTrace;
			var toLog = new LoggerModel
			{
				Type = _type,
				AccountId = _accountId,
				InstagramUsername = _username,
				Message = formatException,
				Date = DateTime.UtcNow
			};
			await _reportHandlerRepository.InsertReport(toLog);
		}
		public async Task MakeReport(ResultInfo resultInfo)
		{
			var formatException = resultInfo.Message + "::" + resultInfo.NeedsChallenge + "::" + resultInfo.Exception + "::" + resultInfo.ResponseType;
			var toLog = new LoggerModel
			{
				Type = _type,
				AccountId = _accountId,
				InstagramUsername = _username,
				Message = formatException,
				Date = DateTime.UtcNow
			};
			await _reportHandlerRepository.InsertReport(toLog);
		}
		public async Task MakeReport(string result)
		{
			var toLog = new LoggerModel
			{
				Type = _type,
				AccountId = _accountId,
				InstagramUsername = _username,
				Message = result,
				Date = DateTime.UtcNow
			};
			await _reportHandlerRepository.InsertReport(toLog);
		}
	}
}

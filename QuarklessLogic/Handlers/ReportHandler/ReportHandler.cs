using InstagramApiSharp.Classes;
using QuarklessContexts.Models.Logger;
using QuarklessRepositories.RepositoryClientManager;
using System;

namespace QuarklessLogic.Handlers.ReportHandler
{
	public class ReportHandler : IReportHandler
	{
		private string _Username;
		private string _AccountId;
		private string _Type;
		private readonly IRepositoryContext _context;

		public ReportHandler(IRepositoryContext context)
		{
			_context = context;
		}

		public void SetupReportHandler(string type = "", string accountId= "", string username="")
		{
			this._Username = username;
			this._AccountId = accountId;
			this._Type = type;
		}

		public async void MakeReport(Exception errors)
		{
			string formatException =  errors.Message + "::" + errors.Source + "::" + errors.StackTrace;
			var toLog = new LoggerModel{
				Type = _Type,
				AccountId = _AccountId,
				InstagramUsername = _Username,
				Message = formatException,
				Date = DateTime.UtcNow
			};
			await _context.Logger.InsertOneAsync(toLog);
		}
		public async void MakeReport(ResultInfo resultInfo)
		{
			string formatException = resultInfo.Message + "::" + resultInfo.NeedsChallenge + "::" + resultInfo.Exception + "::" + resultInfo.ResponseType;
			var toLog = new LoggerModel
			{
				Type = _Type,
				AccountId = _AccountId,
				InstagramUsername = _Username,
				Message = formatException,
				Date = DateTime.UtcNow
			};
			await _context.Logger.InsertOneAsync(toLog);
		}

		public async void MakeReport(string result)
		{
			var toLog = new LoggerModel
			{
				Type = _Type,
				AccountId = _AccountId,
				InstagramUsername = _Username,
				Message = result,
				Date = DateTime.UtcNow
			};
			await _context.Logger.InsertOneAsync(toLog);
		}
	}

}

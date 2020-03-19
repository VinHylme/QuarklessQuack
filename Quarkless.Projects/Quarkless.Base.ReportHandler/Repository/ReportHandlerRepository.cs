using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Quarkless.Base.ReportHandler.Models;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Base.ReportHandler.Repository
{
	public class ReportHandlerRepository : IReportHandlerRepository
	{
		private readonly IMongoCollection<LoggerModel> _ctx;
		private const string COLLECTION_NAME = "ReportHandle";
		public ReportHandlerRepository(IMongoClientContext context)
			=> _ctx = context.ControlDatabase.GetCollection<LoggerModel>(COLLECTION_NAME);

		public async Task InsertReport(LoggerModel logger)
		{
			await _ctx.InsertOneAsync(logger);
		}
		public async Task<IEnumerable<LoggerModel>> GetAllLogs()
		{
			try
			{
				var results = await _ctx.FindAsync(_ => true);
				return results.ToEnumerable();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<LoggerModel>();
			}
		}
	}
}

using Hangfire.Mongo.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.TimelineRepository
{
	public class TimelineRepository : ITimelineRepository
	{
		private readonly IRepositoryContext _context;

		public TimelineRepository(IRepositoryContext context)
		{
			_context = context;
		}
		public async Task<IEnumerable<object>> GetAllEvents()
		{
			try
			{
				var res =  await _context.Timeline.FindAsync(_=>true);
				return res.ToList();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}

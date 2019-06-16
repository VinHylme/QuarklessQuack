using MongoDB.Driver;
using QuarklessContexts.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository
{
	public class CaptionsRepository : ICaptionsRepository
	{
		private readonly IRepositoryContext _context;
		public CaptionsRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}

		public async Task<bool> AddCaptions(IEnumerable<CaptionsModel> captions)
		{
			try { 

				await _context.Captions.InsertManyAsync(captions);
				return true;
			}
			catch(Exception ee)
			{
				return false;
			}
		}

		public async Task<bool> RemoveCaptions(IEnumerable<string> caption_Ids)
		{
			try
			{
				if(caption_Ids==null && caption_Ids.Count()<=0) return false;
				var builders = Builders<CaptionsModel>.Filter.In(item => item._id,caption_Ids);
				var res = await _context.Captions.DeleteManyAsync(builders);
				return res.DeletedCount>0;
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public async Task<IEnumerable<CaptionsModel>> GetCaptions(IEnumerable<FilterDefinition<CaptionsModel>> searchRepository = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<CaptionsModel>> filterList = new List<FilterDefinition<CaptionsModel>>(); 
				var builders = Builders<CaptionsModel>.Filter;

				if (searchRepository == null) {
					filterList.Add(FilterDefinition<CaptionsModel>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}
				var filter = builders.And(filterList);
				var options = new FindOptions<CaptionsModel,CaptionsModel>();
				if(limit!=-1)
					options.Limit = limit;

				var res = await _context.Captions.FindAsync(filter,options);
				return res.ToList();
			}
			catch(Exception e)
			{

				return null;
			}
		}
	}
}

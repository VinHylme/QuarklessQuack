using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.ServicesRepositories
{
	public class PostServicesRepository : IPostServicesRepository
	{
		private readonly IRepositoryContext _context;
		public PostServicesRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}

		public async Task<bool> BulkAdd(IEnumerable<PostServiceModel> posts)
		{
			try
			{
				await _context.PostingService.InsertManyAsync(posts);
				return true;
			}
			catch(Exception ee)
			{
				Console.Write(ee.Message);
				return false;
			}
		}
		public async Task<IEnumerable<PostServiceModel>> RetrievePosts(string topic)
		{
			try
			{
				var builders = Builders<PostServiceModel>.Filter;
				var filter = builders.Eq("Topic",topic);
				var results = await _context.PostingService.FindAsync(filter);
				return results.ToList();
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}

using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository
{
	public class CommentsRepository : ICommentsRepository
	{
		private readonly IRepositoryContext _context;
		public CommentsRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}

		public async void AddComments(IEnumerable<CommentsModel> comments)
		{
			await _context.Comments.InsertManyAsync(comments);
		}
		public async Task<bool> RemoveComments(IEnumerable<string> comment_ids)
		{
			try
			{
				if (comment_ids == null && comment_ids.Count() <= 0) return false;
				var builders = Builders<CommentsModel>.Filter.In(item => item._id, comment_ids);
				var res = await _context.Comments.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
		public async Task<IEnumerable<CommentsModel>> GetComments(IEnumerable<FilterDefinition<CommentsModel>> searchRepository = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<CommentsModel>> filterList = new List<FilterDefinition<CommentsModel>>();
				var builders = Builders<CommentsModel>.Filter;

				if (searchRepository == null)
				{
					filterList.Add(FilterDefinition<CommentsModel>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}
				var filter = builders.And(filterList);
				var options = new FindOptions<CommentsModel, CommentsModel>();
				if (limit != -1)
					options.Limit = limit;

				var res = await _context.Comments.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}
	}
}

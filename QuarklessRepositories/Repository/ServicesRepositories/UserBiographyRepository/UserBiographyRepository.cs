using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.ServicesRepositories.UserBiographyRepository
{
	public class UserBiographyRepository : IUserBiographyRepository
	{
		private readonly IRepositoryContext _context;
		public UserBiographyRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}
		public void AddBiographies(IEnumerable<UserBiographyModel> userBiography)
		{
			_context.UserBiography.InsertMany(userBiography);
		}
		public async Task<bool> RemoveUserBiographies(IEnumerable<string> bio_ids)
		{
			try
			{
				if (bio_ids == null && bio_ids.Count() <= 0) return false;
				var builders = Builders<UserBiographyModel>.Filter.In(item => item._id, bio_ids);
				var res = await _context.UserBiography.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
		public async Task<IEnumerable<UserBiographyModel>> GetUserBiographies(IEnumerable<FilterDefinition<UserBiographyModel>> searchRepository = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<UserBiographyModel>> filterList = new List<FilterDefinition<UserBiographyModel>>();
				var builders = Builders<UserBiographyModel>.Filter;

				if (searchRepository == null)
				{
					filterList.Add(FilterDefinition<UserBiographyModel>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}
				var filter = builders.And(filterList);
				var options = new FindOptions<UserBiographyModel, UserBiographyModel>();
				if (limit != -1)
					options.Limit = limit;

				var res = await _context.UserBiography.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}

	}
}

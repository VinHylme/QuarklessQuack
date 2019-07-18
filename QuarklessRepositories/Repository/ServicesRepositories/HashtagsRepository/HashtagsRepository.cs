using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository
{
	public class HashtagsRepository : IHashtagsRepository
	{
		private readonly IRepositoryContext _context;
		public HashtagsRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}
		public async Task<bool> RemoveHashtags(IEnumerable<string> hashtag_ids)
		{
			try
			{
				if (hashtag_ids == null && hashtag_ids.Count() <= 0) return false;
				var builders = Builders<HashtagsModel>.Filter.In(item => item._id, hashtag_ids);
				var res = await _context.Hashtags.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
		public async Task<bool> AddHashtags(IEnumerable<HashtagsModel> hashtags)
		{
			try
			{
				await _context.Hashtags.InsertManyAsync(hashtags);
				return true;
			}
			catch (Exception ee)
			{
				return false;
			}
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtags(IEnumerable<FilterDefinition<HashtagsModel>> searchRepository = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<HashtagsModel>> filterList = new List<FilterDefinition<HashtagsModel>>();
				var builders = Builders<HashtagsModel>.Filter;

				if (searchRepository == null)
				{
					filterList.Add(FilterDefinition<HashtagsModel>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}
				var filter = builders.And(filterList);
				var options = new FindOptions<HashtagsModel, HashtagsModel>();
				if (limit != -1)
					options.Limit = limit;

				var res = await _context.Hashtags.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtagsByTopic(string topic, int limit)
		{
			var query = new FilterDefinitionBuilder<HashtagsModel>().Eq("Topic", topic.ToLower());
			var countSize = await _context.Hashtags.CountDocumentsAsync(query);
			if (countSize < limit)
			{
				limit = (int) countSize;
			}
			return (await _context.Hashtags.FindAsync(query, new FindOptions<HashtagsModel, HashtagsModel>
			{
				Skip = SecureRandom.Next((int)(await _context.Hashtags.CountDocumentsAsync(query)) - limit),
				Limit = limit
			})).ToList();
		}
	}
}

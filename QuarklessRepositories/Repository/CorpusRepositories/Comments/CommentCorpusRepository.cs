using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Extensions;

namespace QuarklessRepositories.Repository.CorpusRepositories.Comments
{
	public class CommentCorpusRepository : ICommentCorpusRepository
	{
		private readonly IRepositoryContext _context;
		public CommentCorpusRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}
		public async Task AddComments(IEnumerable<CommentCorpus> comments)
		{
			await _context.CorpusComments.InsertManyAsync(comments);
		}
		public async Task<bool> RemoveComments(IEnumerable<string> comment_ids)
		{
			try
			{
				if (comment_ids == null && comment_ids.Count() <= 0) return false;
				var builders = Builders<CommentCorpus>.Filter.In(item => item._id, comment_ids);
				var res = await _context.CorpusComments.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public async Task UpdateAllCommentsLanguagesToLower()
		{
			var res = (await _context.CorpusComments.DistinctAsync(_ => _.Language, _ => true)).ToList();
			foreach (var lang in res)
			{
				var updateDef = Builders<CommentCorpus>.Update.Set(o => o.Language, lang.ToLower());
				await _context.CorpusComments.UpdateManyAsync(_ => _.Language == lang, updateDef);
			}
		}
		public async Task<IEnumerable<CommentCorpus>> GetComments(IEnumerable<FilterDefinition<CommentCorpus>> searchRepository = null, int limit = -1)
		{
			try
			{
				var filterList = new List<FilterDefinition<CommentCorpus>>();
				var builders = Builders<CommentCorpus>.Filter;

				if (searchRepository == null)
				{
					filterList.Add(FilterDefinition<CommentCorpus>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}
				var filter = builders.And(filterList);
				var options = new FindOptions<CommentCorpus, CommentCorpus>();
				if (limit != -1)
					options.Limit = limit;

				var res = await _context.CorpusComments.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}
		public async Task<IEnumerable<CommentCorpus>> GetComments(string topic, string language = null,  int limit = -1, bool skip = true)
		{
			try
			{
				var builders = Builders<CommentCorpus>.Filter;
				FilterDefinition<CommentCorpus> filters;
				if (string.IsNullOrEmpty(language))
				{
					filters = builders.Eq(_ => _.Topic, topic);
				}
				else
				{
					filters = builders.Eq(_ => _.Topic, topic) & (builders.Eq(_ => _.Language, language));
				}

				var len = await _context.CorpusComments.CountDocumentsAsync(filters);
				if(len<=0) return new List<CommentCorpus>();
				var options = new FindOptions<CommentCorpus, CommentCorpus>()
				{
					Skip = skip ? SecureRandom.Next((int)len/2) : 0
				};
				if (limit != -1)
					options.Limit = limit;

				var res = await _context.CorpusComments.FindAsync(filters, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}

		public async Task<long> GetCommentsCount(string topic)
		{
			var builders = Builders<CommentCorpus>.Filter;
			FilterDefinition<CommentCorpus> filters;
			filters = builders.Eq(_ => _.Topic, topic);
			return await _context.CorpusComments.CountDocumentsAsync(filters);
		}
	}
}

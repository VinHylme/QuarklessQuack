using Quarkless.Models.Comments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using Quarkless.Models.Comments;
using Quarkless.Models.Common.Extensions;
using MongoDB.Driver;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Comments
{
	public class CommentCorpusRepository : ICommentCorpusRepository
	{
		private readonly IMongoCollection<CommentCorpus> _ctx;
		private const string COLLECTION_NAME = "CommentCorpus";

		public CommentCorpusRepository(IMongoClientContext context)
			=> _ctx = context.ContentDatabase.GetCollection<CommentCorpus>(COLLECTION_NAME);

		public async Task AddComments(IEnumerable<CommentCorpus> comments)
		{
			await _ctx.InsertManyAsync(comments);
		}
		public async Task<bool> RemoveComments(IEnumerable<string> comment_ids)
		{
			try
			{
				if (comment_ids == null || !comment_ids.Any()) return false;
				var builders = Builders<CommentCorpus>.Filter.In(item => item._id, comment_ids);
				var res = await _ctx.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
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

				var res = await _ctx.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public async Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true)
		{
			var results = new List<CommentCorpus>();
			try
			{
				var filters = Builders<CommentCorpus>.Filter.Eq(_ => _.From.TopicHash, topicHashCode);
				var len = await _ctx.CountDocumentsAsync(filters);
				if (len <= 0) return results;
				var options = new FindOptions<CommentCorpus, CommentCorpus>()
				{
					Skip = skip ? SecureRandom.Next((int)len / 2) : 0
				};
				if (limit != -1)
					options.Limit = limit;
				var res = await _ctx.FindAsync(filters, options);
				results = res.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}

			return results;
		}
		public async Task<IEnumerable<CommentCorpus>> GetComments(string topic, string language = null,  int limit = -1, bool skip = true)
		{
			try
			{
				var builders = Builders<CommentCorpus>.Filter;
				FilterDefinition<CommentCorpus> filters;
				if (string.IsNullOrEmpty(language))
				{
					filters = builders.Eq(_ => _.From.TopicRequest.Name,
						BsonRegularExpression.Create(new Regex("^" + topic + "$", RegexOptions.IgnoreCase)));
				}
				else
				{
					filters = builders.Eq(_ => _.From.TopicRequest.Name, topic) & (builders.Eq(_ => _.Language, language));
				}

				var len = await _ctx.CountDocumentsAsync(filters);
				if(len<=0) return new List<CommentCorpus>();
				var options = new FindOptions<CommentCorpus, CommentCorpus>()
				{
					Skip = skip ? SecureRandom.Next((int)len/2) : 0
				};
				if (limit != -1)
					options.Limit = limit;

				var res = await _ctx.FindAsync(filters, options);
				return res?.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public async Task<long> GetCommentsCount(string topic)
		{
			var builders = Builders<CommentCorpus>.Filter;
			FilterDefinition<CommentCorpus> filters;
			filters = builders.Eq(_ => _.From.TopicRequest.Name, topic);
			return await _ctx.CountDocumentsAsync(filters);
		}
	}
}

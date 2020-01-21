using Quarkless.Models.Hashtag.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Hashtag;
using MongoDB.Driver;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Hashtag
{
	public class HashtagsRepository : IHashtagsRepository
	{
		private readonly IMongoCollection<HashtagsModel> _ctx;
		private const string COLLECTION_NAME = "HashtagCorpus";

		public HashtagsRepository(IMongoClientContext context)
			=> _ctx = context.ContentDatabase.GetCollection<HashtagsModel>(COLLECTION_NAME);

		public async Task<bool> RemoveHashtags(IEnumerable<string> hashtag_ids)
		{
			try
			{
				if (hashtag_ids == null && !hashtag_ids.Any()) return false;
				var builders = Builders<HashtagsModel>.Filter.In(item => item._id, hashtag_ids);
				var res = await _ctx.DeleteManyAsync(builders);
				return res.DeletedCount > 0;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return false;
			}
		}
		public async Task<bool> AddHashtags(IEnumerable<HashtagsModel> hashtags)
		{
			try
			{
				await _ctx.InsertManyAsync(hashtags);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return false;
			}
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtags(int topicHashCode, int limit = -1, bool skip = true)
		{
			var results = new List<HashtagsModel>();
			try
			{
				var filters = Builders<HashtagsModel>.Filter.Eq(_ => _.From.TopicHash, topicHashCode);
				var len = await _ctx.CountDocumentsAsync(filters);
				if (len <= 0) return results;
				var options = new FindOptions<HashtagsModel, HashtagsModel>()
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
		public async Task<IEnumerable<HashtagsModel>> GetHashtags(IEnumerable<FilterDefinition<HashtagsModel>> searchRepository = null, int limit = -1)
		{
			try
			{
				var filterList = new List<FilterDefinition<HashtagsModel>>();
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

				var res = await _ctx.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public async Task<List<HashtagsModel>> GetHashtagsByTopic(string topic, int limit)
		{
			try
			{
				var query = new FilterDefinitionBuilder<HashtagsModel>().Regex(_ => _.From.TopicRequest.Name,
					BsonRegularExpression.Create(new Regex("^" + topic + "$", RegexOptions.IgnoreCase)));
				return (await _ctx.FindAsync(query, new FindOptions<HashtagsModel, HashtagsModel>
				{
					Limit = limit
				})).ToList();

			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<HashtagsModel>();
			}
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtags(string topic, string language = null, string mapedLang = null, int limit = -1)
		{
			try
			{
				var filterList = new List<FilterDefinition<HashtagsModel>>();
				var builders = Builders<HashtagsModel>.Filter;
				FilterDefinition<HashtagsModel> filters;
				if (string.IsNullOrEmpty(language) && string.IsNullOrEmpty(mapedLang))
				{
					filters = builders.Eq(_ => _.From.TopicRequest.Name, topic);
				}
				else
				{
					filters = builders.Eq(_ => _.From.TopicRequest.Name, topic) & (builders.Eq(_ => _.Language, language) | builders.Eq(_ => _.Language, mapedLang));
				}
				var options = new FindOptions<HashtagsModel, HashtagsModel>();
				if (limit != -1)
					options.Limit = limit;

				var res = await _ctx.FindAsync(filters, options);
				return res.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}
}

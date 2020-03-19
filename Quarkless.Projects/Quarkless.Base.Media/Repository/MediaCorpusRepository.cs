using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Base.Media.Models;
using Quarkless.Base.Media.Models.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Base.Media.Repository
{
	public class MediaCorpusRepository : IMediaCorpusRepository
	{
		private readonly IMongoCollection<MediaCorpus> _ctx;
		private const string COLLECTION_NAME = "MediaCorpus";

		public MediaCorpusRepository(IMongoClientContext context)
			=> _ctx = context.ContentDatabase.GetCollection<MediaCorpus>(COLLECTION_NAME);

		public async Task AddMedias(IEnumerable<MediaCorpus> medias)
		{
			await _ctx.InsertManyAsync(medias);
		}

		public async Task AddMedia(MediaCorpus mediaCorpus)
		{
			await _ctx.InsertOneAsync(mediaCorpus);
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(IEnumerable<FilterDefinition<MediaCorpus>> searchRepository = null, int limit = -1)
		{
			try
			{
				var filterList = new List<FilterDefinition<MediaCorpus>>();
				var builders = Builders<MediaCorpus>.Filter;

				if (searchRepository == null)
				{
					filterList.Add(FilterDefinition<MediaCorpus>.Empty);
				}
				else
				{
					filterList.AddRange(searchRepository);
				}

				var filter = builders.And(filterList);
				var options = new FindOptions<MediaCorpus, MediaCorpus>();
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
		public async Task<long> GetMediasCount(string topic)
		{
			var builders = Builders<MediaCorpus>.Filter;
			FilterDefinition<MediaCorpus> filters;
			filters = builders.Eq(_ => _.From.TopicRequest.Name, topic);
			return await _ctx.CountDocumentsAsync(filters);
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(int topicHashCode, int limit = -1, bool skip = true)
		{
			var results = new List<MediaCorpus>();
			try
			{
				var filters = Builders<MediaCorpus>.Filter.Eq(_ => _.From.TopicHash, topicHashCode);
				var len = await _ctx.CountDocumentsAsync(filters);
				if (len <= 0) return results;
				var options = new FindOptions<MediaCorpus, MediaCorpus>()
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
		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string language = null, 
			int limit = -1, bool skip = true)
		{
			try
			{
				var builders = Builders<MediaCorpus>.Filter;
				FilterDefinition<MediaCorpus> filters;

				if(string.IsNullOrEmpty(language))
				{
					filters = builders.Regex(_ => _.From.TopicRequest.Name, 
						BsonRegularExpression.Create(new Regex("^" + topic + "$", RegexOptions.IgnoreCase)));
				}
				else
				{

					filters = builders.Eq(_ => _.From.TopicRequest.Name, topic) & (builders.Eq(_ => _.Language, language));
				}

				var len = await _ctx.CountDocumentsAsync(filters);

				if(len<=0) return new List<MediaCorpus>();
				var options = new FindOptions<MediaCorpus, MediaCorpus>()
				{
					Skip = skip ? SecureRandom.Next((int)len/2) : 0
				};

				if (limit != -1)
					options.Limit = limit;
				var res = await _ctx.FindAsync(filters, options);
				return res.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
	}
}

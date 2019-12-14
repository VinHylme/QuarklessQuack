using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.CorpusModels;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.CorpusRepositories.Medias
{
	public class MediaCorpusRepository : IMediaCorpusRepository
	{
		private readonly IRepositoryContext _context;
		public MediaCorpusRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}
		public async Task AddMedias(IEnumerable<MediaCorpus> medias)
		{
			await _context.CorpusMedia.InsertManyAsync(medias);
		}

		public async Task AddMedia(MediaCorpus mediaCorpus)
		{
			await _context.CorpusMedia.InsertOneAsync(mediaCorpus);
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(IEnumerable<FilterDefinition<MediaCorpus>> searchRepository = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<MediaCorpus>> filterList = new List<FilterDefinition<MediaCorpus>>();
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

				var res = await _context.CorpusMedia.FindAsync(filter, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}
		public async Task<long> GetMediasCount(string topic)
		{
			var builders = Builders<MediaCorpus>.Filter;
			FilterDefinition<MediaCorpus> filters;
			filters = builders.Eq(_ => _.Topic, topic);
			return await _context.CorpusMedia.CountDocumentsAsync(filters);
		}

		public async Task UpdateTopicName(string topic, string newTopic)
		{
			var updateDef = Builders<MediaCorpus>.Update.Set(o => o.Topic, newTopic);
			var updateDef2 = Builders<CommentCorpus>.Update.Set(o => o.Topic, newTopic);
			var updateDef3 = Builders<HashtagsModel>.Update.Set(o => o.Topic, newTopic);

			await _context.CorpusMedia.UpdateManyAsync(o => o.Topic == topic, updateDef);
			await _context.CorpusComments.UpdateManyAsync(o => o.Topic == topic, updateDef2);
			await _context.Hashtags.UpdateManyAsync(o => o.Topic == topic, updateDef3);
		}

		public async Task UpdateAllMediasLanguagesToLower()
		{
			var res = (await _context.CorpusMedia.DistinctAsync(_ => _.Language, _ => true)).ToList();
			foreach (var lang in res)
			{
				var updateDef = Builders<MediaCorpus>.Update.Set(o => o.Language, lang.ToLower());
				await _context.CorpusMedia.UpdateManyAsync(_ => _.Language == lang, updateDef);
			}
		}

		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string language = null, int limit = -1, bool skip = true)
		{
			try
			{
				var builders = Builders<MediaCorpus>.Filter;
				FilterDefinition<MediaCorpus> filters;
				if(string.IsNullOrEmpty(language))
				{
					filters = builders.Eq(_ => _.Topic, topic);
				}
				else
				{

					filters = builders.Eq(_ => _.Topic, topic) & (builders.Eq(_ => _.Language, language));
				}
				var len = await _context.CorpusMedia.CountDocumentsAsync(filters);
				if(len<=0) return new List<MediaCorpus>();
				var options = new FindOptions<MediaCorpus, MediaCorpus>()
				{
					Skip = skip ? SecureRandom.Next((int)len/2) : 0
				};
				if (limit != -1)
					options.Limit = limit;
				var res = await _context.CorpusMedia.FindAsync(filters, options);
				return res.ToList();
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
			{

				return null;
			}
		}


	}
}

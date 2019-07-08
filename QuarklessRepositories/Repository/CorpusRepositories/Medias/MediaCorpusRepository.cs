using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string language = null ,string mapedLang = null, int limit = -1)
		{
			try
			{
				List<FilterDefinition<MediaCorpus>> filterList = new List<FilterDefinition<MediaCorpus>>();
				var builders = Builders<MediaCorpus>.Filter;
				FilterDefinition<MediaCorpus> filters;
				if(string.IsNullOrEmpty(language) && string.IsNullOrEmpty(mapedLang))
				{
					filters = builders.Eq(_ => _.Topic, topic);
				}
				else
				{
					filters = builders.Eq(_ => _.Topic, topic) & (builders.Eq(_ => _.Language, language) | builders.Eq(_ => _.Language, mapedLang));
				}
				var options = new FindOptions<MediaCorpus, MediaCorpus>();
				if (limit != -1)
					options.Limit = limit;
				var res = await _context.CorpusMedia.FindAsync(filters, options);
				return res.ToList();
			}
			catch (Exception e)
			{

				return null;
			}
		}


	}
}

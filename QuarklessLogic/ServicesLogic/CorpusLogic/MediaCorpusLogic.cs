using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.ServicesLogic.CorpusLogic
{
	public class MediaCorpusLogic : IMediaCorpusLogic
	{
		private readonly IMediaCorpusCache _mediaCorpusCache;
		private readonly IMediaCorpusRepository _mediaCorpusRepository;
		public MediaCorpusLogic(IMediaCorpusCache mediaCorpusCache, IMediaCorpusRepository mediaCorpusRepository)
		{
			_mediaCorpusCache = mediaCorpusCache;
			_mediaCorpusRepository = mediaCorpusRepository;
		}

		public async Task AddMedias(IEnumerable<MediaCorpus> medias)
		{
			await _mediaCorpusRepository.AddMedias(medias);
			await _mediaCorpusCache.AddMedias(medias);
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, string langmapped, int limit)
		{
			var medias = await _mediaCorpusCache.GetMedias(topic, langmapped, limit);
			if (medias != null && medias.Count() > 0)
				return medias;
			else
				return await _mediaCorpusRepository.GetMedias(topic, lang, langmapped, limit);
		}

		public async Task<long> MediasCount(string topic) => await _mediaCorpusRepository.GetMediasCount(topic);
	}
}

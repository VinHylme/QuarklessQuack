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
			//await _mediaCorpusCache.AddMedias(medias);
		}

		public async Task UpdateTopicName(string topic, string newTopic)
		{
			await _mediaCorpusRepository.UpdateTopicName(topic, newTopic);
		}
		public async Task<IEnumerable<MediaCorpus>> GetMedias(string topic, string lang, string langmapped, int limit)
		{
			var medias = await _mediaCorpusCache.GetMedias(topic, langmapped, limit);
			var mediaCorpora = medias as MediaCorpus[] ?? medias.ToArray();
			if (mediaCorpora.Any())
				return mediaCorpora;
			return await _mediaCorpusRepository.GetMedias(topic, lang, langmapped, limit);
		}

		public async Task<long> MediasCount(string topic) => await _mediaCorpusRepository.GetMediasCount(topic);
	}
}

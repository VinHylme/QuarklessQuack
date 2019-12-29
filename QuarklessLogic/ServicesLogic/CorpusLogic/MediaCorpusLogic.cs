using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using System.Collections.Generic;
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
		}

		public Task AddMedia(MediaCorpus mediaCorpus) => _mediaCorpusRepository.AddMedia(mediaCorpus);

		public async Task<IEnumerable<MediaCorpus>> GetMedias(int topicHash, int limit = -1, bool skip = true)
		{
			return await _mediaCorpusRepository.GetMedias(topicHash, limit, skip);
		}

		public async Task<long> MediasCount(string topic) => await _mediaCorpusRepository.GetMediasCount(topic);
	}
}

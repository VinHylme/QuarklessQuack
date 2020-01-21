using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Media;
using Quarkless.Models.Media.Interfaces;

namespace Quarkless.Logic.Media
{
	public class MediaCorpusLogic : IMediaCorpusLogic
	{
		private readonly IMediaCorpusRepository _mediaCorpusRepository;

		public MediaCorpusLogic(IMediaCorpusRepository mediaCorpusRepository)
		{
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
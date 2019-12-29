using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.ServicesLogic.CorpusLogic
{
	public class CommentCorpusLogic : ICommentCorpusLogic
	{
		private readonly ICommentCorpusRepository _commentCorpusRepository;
		private readonly ICommentCorpusCache _commentCorpusCache;
		public CommentCorpusLogic(ICommentCorpusRepository commentCorpusRepository, ICommentCorpusCache commentCorpusCache)
		{
			_commentCorpusRepository = commentCorpusRepository;
			_commentCorpusCache = commentCorpusCache;
		}

		public async Task AddComments(IEnumerable<CommentCorpus> comments)
		{
			await _commentCorpusRepository.AddComments(comments);
			//await _commentCorpusCache.AddComments(comments);
		}

		public async Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true)
		{
			return await _commentCorpusRepository.GetComments(topicHashCode, limit, skip);
		}

//		public async Task<IEnumerable<CommentCorpus>> GetComments(string topic, string lang, int limit = -1, bool skip = true)
//		{
//			return await _commentCorpusRepository.GetComments(topic, lang, limit, skip);
//		}
		public async Task<long> CommentsCount(string topic) => await _commentCorpusRepository.GetCommentsCount(topic);

	}
}

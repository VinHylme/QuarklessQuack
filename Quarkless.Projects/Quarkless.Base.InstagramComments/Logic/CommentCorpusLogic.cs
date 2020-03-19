using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.InstagramComments.Models;
using Quarkless.Base.InstagramComments.Models.Interfaces;

namespace Quarkless.Base.InstagramComments.Logic
{
	public class CommentCorpusLogic : ICommentCorpusLogic
	{
		private readonly ICommentCorpusRepository _commentCorpusRepository;
		public CommentCorpusLogic(ICommentCorpusRepository commentCorpusRepository)
		{
			_commentCorpusRepository = commentCorpusRepository;
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
		public async Task<long> CommentsCount(string topic) => await _commentCorpusRepository.GetCommentsCount(topic);
	}
}

using Quarkless.Models.Comments;
using Quarkless.Models.Comments.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Logic.Comments
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

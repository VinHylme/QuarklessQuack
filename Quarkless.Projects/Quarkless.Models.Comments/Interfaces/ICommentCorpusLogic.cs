using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Comments.Interfaces
{
	public interface ICommentCorpusLogic
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);
		Task<long> CommentsCount(string topic);
		Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true);
	}
}
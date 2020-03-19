using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.InstagramComments.Models.Interfaces
{
	public interface ICommentCorpusLogic
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);
		Task<long> CommentsCount(string topic);
		Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true);
	}
}
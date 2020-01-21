using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Comments.Interfaces
{
	public interface ICommentCorpusRepository
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);
		Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true); 
		Task<bool> RemoveComments(IEnumerable<string> comment_ids);
		Task<long> GetCommentsCount(string topic);
	}
}
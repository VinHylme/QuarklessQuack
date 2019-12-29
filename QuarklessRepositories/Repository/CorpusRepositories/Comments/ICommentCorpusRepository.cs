using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessRepositories.Repository.CorpusRepositories.Comments
{
	public interface ICommentCorpusRepository
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);

		Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true);
		//Task<IEnumerable<CommentCorpus>> GetComments(IEnumerable<FilterDefinition<CommentCorpus>> searchRepository = null, int limit = -1);
		//Task<IEnumerable<CommentCorpus>> GetComments(string topic, string language = null, int limit = -1, bool skip = true);
		Task<bool> RemoveComments(IEnumerable<string> comment_ids);
		Task<long> GetCommentsCount(string topic);
	}
}
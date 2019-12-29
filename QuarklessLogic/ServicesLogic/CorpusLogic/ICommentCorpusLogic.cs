using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessLogic.ServicesLogic.CorpusLogic
{
	public interface ICommentCorpusLogic
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);
		Task<long> CommentsCount(string topic);

		Task<IEnumerable<CommentCorpus>> GetComments(int topicHashCode, int limit = -1, bool skip = true);
		//Task<IEnumerable<CommentCorpus>> GetComments(string topic, string lang, int limit = -1, bool skip = true);
	}
}
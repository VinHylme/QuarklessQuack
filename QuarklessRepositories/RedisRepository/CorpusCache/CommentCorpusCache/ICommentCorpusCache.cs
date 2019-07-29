using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache
{
	public interface ICommentCorpusCache
	{
		Task AddComments(IEnumerable<CommentCorpus> comments);
		Task<IEnumerable<CommentCorpus>> GetComments(string topic, string lang, int limit);
	}
}
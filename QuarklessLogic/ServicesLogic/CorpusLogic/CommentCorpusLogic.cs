using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public async Task<IEnumerable<CommentCorpus>> GetComments(string topic, string lang, string mappedLang, int limit)
		{
			//var cacheRes = await _commentCorpusCache.GetComments(topic, mappedLang, limit);
			//var commentCorpora = cacheRes as CommentCorpus[] ?? cacheRes.ToArray();
			//if (commentCorpora.Any())
				//return commentCorpora;
			return await _commentCorpusRepository.GetComments(topic, lang, mappedLang, limit);
		}

		public async Task UpdateAllCommentsLanguagesToLower() => await _commentCorpusRepository.UpdateAllCommentsLanguagesToLower();

		public async Task<long> CommentsCount(string topic) => await _commentCorpusRepository.GetCommentsCount(topic);
		

	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository
{
	public interface ICommentsRepository : IServiceRepository
	{
		void AddComments(IEnumerable<CommentsModel> comments);
		Task<bool> RemoveComments(IEnumerable<string> comment_ids);
		Task<IEnumerable<CommentsModel>> GetComments(IEnumerable<FilterDefinition<CommentsModel>> searchRepository = null, int limit = -1);

	}
}
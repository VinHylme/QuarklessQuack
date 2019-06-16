using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels;

namespace QuarklessRepositories.Repository.ServicesRepositories
{
	public interface IPostServicesRepository : IServiceRepository
	{
		Task<bool> BulkAdd(IEnumerable<PostServiceModel> posts);
		Task<IEnumerable<PostServiceModel>> RetrievePosts(string topic);
	}
}
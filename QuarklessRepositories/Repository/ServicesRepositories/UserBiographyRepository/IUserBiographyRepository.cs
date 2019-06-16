using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.ServicesRepositories.UserBiographyRepository
{
	public interface IUserBiographyRepository : IServiceRepository
	{
		void AddBiographies(IEnumerable<UserBiographyModel> userBiography);
		Task<bool> RemoveUserBiographies(IEnumerable<string> bio_ids);
		Task<IEnumerable<UserBiographyModel>> GetUserBiographies(IEnumerable<FilterDefinition<UserBiographyModel>> searchRepository = null, int limit = -1);

	}
}
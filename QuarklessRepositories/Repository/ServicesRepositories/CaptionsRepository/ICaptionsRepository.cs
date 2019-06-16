using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QuarklessContexts.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;

namespace QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository
{
	public interface ICaptionsRepository : IServiceRepository
	{
		Task<bool> AddCaptions(IEnumerable<CaptionsModel> captions);
		Task<IEnumerable<CaptionsModel>> GetCaptions(IEnumerable<FilterDefinition<CaptionsModel>> searchRepository = null, int limit = -1);
		Task<bool> RemoveCaptions(IEnumerable<string> caption_Ids);
	}
}
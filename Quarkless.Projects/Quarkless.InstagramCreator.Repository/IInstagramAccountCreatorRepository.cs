using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.InstagramCreator.Models;

namespace Quarkless.InstagramCreator.Repository
{
	public interface IInstagramAccountCreatorRepository
	{
		Task AddAccount(InstagramAccount account);
		Task<List<InstagramAccount>> GetAllInstagramAccounts();
		Task<List<InstagramAccount>> GetAllNonVerifiedInstagramAccounts();
		Task<InstagramAccount> GetInstagramAccount(string id);
	}
}
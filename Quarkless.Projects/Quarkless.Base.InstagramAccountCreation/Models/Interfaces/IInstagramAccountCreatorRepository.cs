using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.InstagramAccountCreation.Models.Interfaces
{
	public interface IInstagramAccountCreatorRepository
	{
		Task AddAccount(InstagramAccount account);
		Task<List<InstagramAccount>> GetAllInstagramAccounts();
		Task<List<InstagramAccount>> GetAllNonVerifiedInstagramAccounts();
		Task<InstagramAccount> GetInstagramAccount(string id);
	}
}
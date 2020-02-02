using System.Threading.Tasks;
using Quarkless.Models.Auth.AccountContext;

namespace Quarkless.Models.Auth.Interfaces
{
	public interface IAccountRepository
	{
		Task<AccountUser> GetAccountByUsername(string username);
		Task<AccountUser> GetAccountById(string id);
		Task<bool> UpdateAddAccountUserInformation(string id, UserInformationDetail detail);
	}
}
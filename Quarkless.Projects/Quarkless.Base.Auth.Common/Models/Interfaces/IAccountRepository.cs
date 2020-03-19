using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models.AccountContext;

namespace Quarkless.Base.Auth.Common.Models.Interfaces
{
	public interface IAccountRepository
	{
		Task<AccountUser> GetAccountByUsername(string username);
		Task<AccountUser> GetAccountById(string id);
		Task<bool> UpdateAddAccountUserInformation(string id, UserInformationDetail detail);
	}
}
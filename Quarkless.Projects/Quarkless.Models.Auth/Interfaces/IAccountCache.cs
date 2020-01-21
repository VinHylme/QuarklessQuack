using System.Threading.Tasks;
using Quarkless.Models.Auth.AccountContext;

namespace Quarkless.Models.Auth.Interfaces
{
	public interface IAccountCache
	{
		Task<AccountUser> GetAccount(string userId);
		Task SetAccount(AccountUser value);
	}
}

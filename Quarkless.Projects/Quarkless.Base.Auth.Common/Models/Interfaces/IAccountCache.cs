using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models.AccountContext;

namespace Quarkless.Base.Auth.Common.Models.Interfaces
{
	public interface IAccountCache
	{
		Task<AccountUser> GetAccount(string userId);
		Task SetAccount(AccountUser value);
	}
}

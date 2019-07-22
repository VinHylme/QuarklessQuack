using System.Threading.Tasks;
using QuarklessContexts.Contexts.AccountContext;

namespace QuarklessRepositories.RedisRepository.AccountCache
{
	public interface IAccountCache
	{
		Task<AccountUser> GetAccount(string userId);
		Task SetAccount(AccountUser value);
	}
}
using System.Threading.Tasks;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.Interfaces;
namespace Quarkless.Logic.Details
{
	public class AccountDetailLogic : IAccountDetailLogic
	{
		private readonly IAccountRepository _accountRepository;
		public AccountDetailLogic(IAccountRepository accountRepository)
		{
			_accountRepository = accountRepository;
		}

		public async Task<bool> AddOrUpdateAccountDetails(string accountId, UserInformationDetail detail)
			=> await _accountRepository.UpdateAddAccountUserInformation(accountId, detail);
	}
}

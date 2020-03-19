using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Models.Interfaces;

namespace Quarkless.Base.AuthDetails.Logic
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

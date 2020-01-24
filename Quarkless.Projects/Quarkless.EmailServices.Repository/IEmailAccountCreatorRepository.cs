using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.EmailServices.Models;
using Quarkless.SmsHandler.Models.Enums;

namespace Quarkless.EmailServices.Repository
{
	public interface IEmailAccountCreatorRepository
	{
		Task AddAccount(EmailAccount account);
		Task<bool> UpdateExistingService(string emailId, string serviceId, UsedBy updated);
		Task<string> AddService(string id, UsedBy service);
		Task<bool> RemoveService(string emailId, string serviceId);
		Task<List<EmailAccount>> GetAllEmailAccounts();
		Task<List<EmailAccount>> GetUnusedAccounts(ByService notUsedBy);
		Task<EmailAccount> GetGmailAccount(string id);
	}
}
using System.Collections.Generic;
using Quarkless.EmailServices.Models;
using Quarkless.SmsHandler.Models.Enums;
using Quarkless.Utilities.Models.Person;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Quarkless.EmailServices.Logic.GmailService
{
	public interface IGmailClient
	{
		Task<List<GmailMessagesResponse>> GetInstagramMessages(GmailLoginRequest gmailLoginRequest,
			bool stopOnFirst = true, bool verify = true, bool ignoreLoginNotices = true);
		Task<EmailAccount> CreateGmailAccount(PersonCreateModel person);
		Task<List<GmailMessagesResponse>> GetEmailBySubjectName(Browser browser, GmailLoginRequest loginRequest, ISearchRequest searchRequest);
		Task<List<GmailMessagesResponse>> GetInstagramMessages(Browser browser, GmailLoginRequest gmailLoginRequest,
			bool stopOnFirst = true, bool verify = true, bool ignoreLoginNotices = true);

		Task<List<EmailAccount>> GetUnusedEmailAccounts(ByService notUsedByService);
		Task<string> AddServiceToEmail(string emailId, UsedBy usedBy);
		Task<bool> RemoveEmailService(string emailId, string serviceId);
		Task<bool> UpdateExistingService(string emailId, string serviceId, UsedBy usedBy);
		Task<List<EmailAccount>> GetAllEmailAccounts();
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.EmailService.Interfaces
{
	public interface IEmailService
	{
		Task<List<string>> GetUnreadEmails(string email, string password);
	}
}

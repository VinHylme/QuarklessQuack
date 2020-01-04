using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.EmailService
{
	public interface IEmailService
	{
		Task<List<string>> GetUnreadEmails(string email, string password);
	}
}
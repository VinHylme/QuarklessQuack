using Quarkless.SmsHandler.Models;
using System.Threading.Tasks;

namespace Quarkless.SmsHandler.Logic
{
	public interface ISmsService
	{
		Task<Status> GetVerificationCode(int tZid);
		Task<Status> IssueNumber();
		Task<Status> IssueNumberNew();
	}
}
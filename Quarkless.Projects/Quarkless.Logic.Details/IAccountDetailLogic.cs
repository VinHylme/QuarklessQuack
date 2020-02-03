using System.Threading.Tasks;
using Quarkless.Models.Auth;

namespace Quarkless.Logic.Details
{
	public interface IAccountDetailLogic
	{
		Task<bool> AddOrUpdateAccountDetails(string accountId, UserInformationDetail detail);
	}
}
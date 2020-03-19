using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models;

namespace Quarkless.Base.AuthDetails.Models.Interfaces
{
	public interface IAccountDetailLogic
	{
		Task<bool> AddOrUpdateAccountDetails(string accountId, UserInformationDetail detail);
	}
}
using Quarkless.Models.Auth.Enums;

namespace Quarkless.Models.Auth.Interfaces
{
	public interface IUserContext
	{
		bool IsAdmin { get; }
		string CurrentUser { get; }
		string UserIpAddress { get; }
		AuthTypes UserRoleLevel { get; }
		string FocusInstaAccount { get; set; }
		bool UserAccountExists { get; }
	}
}
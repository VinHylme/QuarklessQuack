using Quarkless.Base.Auth.Common.Models.Enums;

namespace Quarkless.Base.Auth.Common.Models.Interfaces
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
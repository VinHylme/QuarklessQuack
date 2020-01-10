using QuarklessContexts.Models.UserAuth.AuthTypes;

namespace QuarklessContexts.Contexts
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
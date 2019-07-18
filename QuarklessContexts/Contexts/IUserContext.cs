using MongoDB.Bson;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using System.Collections;

namespace QuarklessContexts.Contexts
{
	public interface IUserContext
	{
		bool IsAdmin { get; }
		string CurrentUser { get; }
		AuthTypes UserRoleLevel { get; }
		string FocusInstaAccount { get; set; }
		bool UserAccountExists { get; }
	}
}
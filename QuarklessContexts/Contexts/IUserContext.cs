using MongoDB.Bson;
using System.Collections;

namespace QuarklessContexts.Contexts
{
	public interface IUserContext
	{
		string CurrentUser { get; }
		string UserRoleLevel { get; }
		string FocusInstaAccount { get; set; }
		bool UserAccountExists { get; }
	}
}
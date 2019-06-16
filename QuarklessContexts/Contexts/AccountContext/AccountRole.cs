using AspNetCore.Identity.MongoDbCore.Models;

namespace QuarklessContexts.Contexts.AccountContext
{
	public class AccountRole : MongoIdentityRole<string>
	{
		public AccountRole() : base()
		{
		}

		public AccountRole(string roleName) : base(roleName)
		{
		}
	}
}

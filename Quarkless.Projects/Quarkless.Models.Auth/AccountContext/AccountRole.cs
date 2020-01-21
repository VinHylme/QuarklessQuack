using AspNetCore.Identity.MongoDbCore.Models;

namespace Quarkless.Models.Auth.AccountContext
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

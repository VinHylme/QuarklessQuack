using AspNetCore.Identity.MongoDbCore.Models;

namespace Quarkless.Base.Auth.Common.Models.AccountContext
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

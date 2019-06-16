using AspNetCore.Identity.MongoDbCore.Models;

namespace QuarklessContexts.Contexts.AccountContext
{
	public class AccountUser : MongoIdentityUser<string>
	{
		public bool IsUserConfirmed { get; set; }
		public string CurrentInstagramUser { get; set; }
		public string Sub { get; set; }

		public AccountUser() : base()
		{
		}

		public AccountUser(string userName, string email) : base(userName, email)
		{
		}
		
	}
}

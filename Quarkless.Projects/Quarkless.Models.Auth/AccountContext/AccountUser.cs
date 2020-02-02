using System;
using System.Collections.Generic;
using AspNetCore.Identity.MongoDbCore.Models;

namespace Quarkless.Models.Auth.AccountContext
{
	public class AccountUser : MongoIdentityUser<string>
	{
		public bool IsUserConfirmed { get; set; }
		public List<UserInformationDetail> Details { get; set; } = new List<UserInformationDetail>();
		public DateTime LastLoggedIn { get; set; }
		public string Sub { get; set; }

		public AccountUser() : base()
		{
		}

		public AccountUser(string userName, string email) : base(userName, email)
		{
		}
	}
}

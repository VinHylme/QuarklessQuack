using AspNetCore.Identity.MongoDbCore.Models;
using System;
using System.Collections.Generic;

namespace QuarklessContexts.Contexts.AccountContext
{
	public class AssociatedBrowser
	{
		public string HashCode { get; set; }
		public DateTime AddDate { get; set; }
		public DateTime LastLogged { get; set; }
	}
	public class AccountUser : MongoIdentityUser<string>
	{
		public bool IsUserConfirmed { get; set; }
		public List<string> AssociatedIps { get; set; }
		public List<AssociatedBrowser> AssociatedBrowser { get; set; }
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

using AspNetCore.Identity.MongoDbCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth_.User
{
	public class UserApplication : MongoIdentityUser<Guid>
	{
		public UserApplication() : base()
		{
		}

		public UserApplication(string userName, string email) : base(userName, email)
		{
		}
	}
}

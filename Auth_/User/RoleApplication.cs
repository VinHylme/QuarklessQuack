using AspNetCore.Identity.MongoDbCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth_.User
{
	public class RoleApplication : MongoIdentityRole<Guid>
	{
		public RoleApplication() : base()
		{
		}

		public RoleApplication(string roleName) : base(roleName)
		{
		}
	}
}

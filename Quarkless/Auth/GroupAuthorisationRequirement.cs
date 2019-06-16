using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Auth
{
	public class GroupAuthorisationRequirement : IAuthorizationRequirement
	{
		public string GroupName { get; private set; }
		public GroupAuthorisationRequirement(string groupName)
		{
			this.GroupName = groupName;
		}
	}
}

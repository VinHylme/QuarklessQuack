using Microsoft.AspNetCore.Authorization;

namespace Quarkless.Base.Auth.Logic
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

using Microsoft.AspNetCore.Authorization;

namespace QuarklessLogic.Logic.AuthLogic.Auth
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

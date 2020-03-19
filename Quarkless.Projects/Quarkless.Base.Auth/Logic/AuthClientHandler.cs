using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Quarkless.Base.Auth.Logic
{
	public class AuthClientHandler : AuthorizationHandler<GroupAuthorisationRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupAuthorisationRequirement requirement)
		{
			if (context.User.HasClaim(c => c.Type == "cognito:groups" && c.Value == requirement.GroupName))
			{
				context.Succeed(requirement);
			}
			else
			{
				context.Fail();
			}
			return Task.CompletedTask;
		}
	}
}

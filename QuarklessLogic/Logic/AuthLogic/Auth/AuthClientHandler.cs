using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AuthLogic.Auth
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

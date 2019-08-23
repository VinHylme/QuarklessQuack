using System.Linq;
using Microsoft.AspNetCore.Http;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessContexts.Extensions;

namespace QuarklessContexts.Contexts
{
	public class UserContext : IUserContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UserContext(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
		public string CurrentUser 
		{
			get {
				if (_httpContextAccessor.HttpContext == null)
					return null;
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					return _httpContextAccessor.HttpContext.User.Claims?.Single(_ => _.Type == "cognito:username").Value;
				}

				return null;
			}
		}
		public AuthTypes UserRoleLevel
		{
			get
			{
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					return _httpContextAccessor.HttpContext.User.Claims?.Single(_ => _.Type == "cognito:groups").Value.GetValueFromDescription<AuthTypes>() ?? AuthTypes.Expired;
				}

				return AuthTypes.Expired;
			}
		}

		public string FocusInstaAccount
		{
			get
			{
				if(_httpContextAccessor.HttpContext==null) return null;
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					return _httpContextAccessor.HttpContext.Request.Headers["FocusInstaAccount"].FirstOrDefault();				
				}

				return null;
			}
			set { }
		}
		public bool IsAdmin => !string.IsNullOrEmpty(CurrentUser) && UserRoleLevel == AuthTypes.Admin;

		public bool UserAccountExists => CurrentUser != null && FocusInstaAccount != null;
	}
}

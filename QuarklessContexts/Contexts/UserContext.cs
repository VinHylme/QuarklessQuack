using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessContexts.Extensions;
using System;

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
					{ return null;}
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					if (_httpContextAccessor.HttpContext.User.Claims != null) {

						return _httpContextAccessor.HttpContext.User.Claims.Where(_=>_.Type == "cognito:username").Single().Value;
					}
					return null;
				}
				else
				{
					return null;
				}
			}
		}
		public AuthTypes UserRoleLevel
		{
			get
			{
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					if (_httpContextAccessor.HttpContext.User.Claims != null)
					{

						return _httpContextAccessor.HttpContext.User.Claims.Where(_ => _.Type == "cognito:groups").Single().Value.GetValueFromDescription<AuthTypes>();
					}
					return AuthTypes.Expired;
				}
				else
				{
					return AuthTypes.Expired;
				}
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
				else
				{
					return null;
				}
			}
			set { }
		}
		public bool IsAdmin
		{
			get
			{
				return CurrentUser!=null && UserRoleLevel == AuthTypes.Admin;
			}
		}
		public bool UserAccountExists
		{
			get
			{
				return CurrentUser != null && FocusInstaAccount != null;
			}
		}

	}
}

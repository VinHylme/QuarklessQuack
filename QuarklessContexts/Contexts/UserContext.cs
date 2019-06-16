using System.Linq;
using Microsoft.AspNetCore.Http;

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
		public string UserRoleLevel
		{
			get
			{
				if (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
				{
					if (_httpContextAccessor.HttpContext.User.Claims != null)
					{

						return _httpContextAccessor.HttpContext.User.Claims.Where(_ => _.Type == "cognito:groups").Single().Value;
					}
					return null;
				}
				else
				{
					return null;
				}
			}
		}

		public string FocusInstaAccount
		{
			get
			{
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
		public bool UserAccountExists
		{
			get
			{
				return CurrentUser != null && FocusInstaAccount != null;
			}
		}

	}
}

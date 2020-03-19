using Microsoft.AspNetCore.Authorization;
using System;

namespace Quarkless.Base.Auth.Common.Models.Enums
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class HashtagAuthorize: AuthorizeAttribute
	{
		private readonly string _role;
		public HashtagAuthorize() { }
		public HashtagAuthorize(AuthTypes authTypes)
		{
			_role = authTypes.ToString();
		}
	}
}

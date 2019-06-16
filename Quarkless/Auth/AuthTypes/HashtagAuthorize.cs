using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Auth.AuthTypes
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

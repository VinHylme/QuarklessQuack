using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models.Auth
{
	public class RefreshTokenRequest
	{
		public string refreshToken { get; set; }
		public string Username { get; set;}
	}
}

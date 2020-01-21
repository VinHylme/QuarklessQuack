using System.Collections.Generic;

namespace Quarkless.Models.ApiLogger
{
	public class UserDetail
	{
		public string Ip { get; set; }
		public IEnumerable<IdentityUser> Identity { get; set; }
	}
}
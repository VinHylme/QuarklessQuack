using System.Collections.Generic;

namespace Quarkless.Base.ApiLogger.Models
{
	public class UserDetail
	{
		public string Ip { get; set; }
		public IEnumerable<IdentityUser> Identity { get; set; }
	}
}
using System;
using System.Collections.Generic;

namespace Quarkless.Filters
{
	public class SecurityHeadersPolicy
	{
		public IDictionary<string, string> SetHeaders { get; } 
			 = new Dictionary<string, string>();

		public ISet<string> RemoveHeaders { get; } 
			= new HashSet<string>();
	}
}

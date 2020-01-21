using System;
using System.Collections.Generic;

namespace Quarkless.Models.ApiLogger
{
	[Serializable]
	public class IpRateLimitPolicies
	{
		public List<IpRules> IpRules { get; set; }
	}
}
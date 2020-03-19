using System;
using System.Collections.Generic;

namespace Quarkless.Base.ApiLogger.Models
{
	[Serializable]
	public class IpRateLimitPolicies
	{
		public List<IpRules> IpRules { get; set; }
	}
}
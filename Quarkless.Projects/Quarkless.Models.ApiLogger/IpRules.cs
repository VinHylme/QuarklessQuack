using System;
using System.Collections.Generic;

namespace Quarkless.Models.ApiLogger
{
	[Serializable]
	public class IpRules
	{
		public List<GeneralRule> GeneralRules { get; set; }
		public string Ip { get; set; }
	}
}
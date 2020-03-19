using System;
using System.Collections.Generic;

namespace Quarkless.Base.ApiLogger.Models
{
	[Serializable]
	public class IpRateLimiting
	{
		public bool EnableEndpointRateLimiting { get; set; }
		public bool StackBlockedRequests { get; set; }
		public string RealIpHeader { get; set; }
		public string ClientIdHeader { get; set; }
		public int HttpStatusCode { get; set; }
		public List<string> IpWhitelist { get; set; }
		public List<string> EndpointWhitelist { get; set; }
		public List<string> ClientWhitelist { get; set; }
		public List<GeneralRule> GeneralRules { get; set; }
	}
}
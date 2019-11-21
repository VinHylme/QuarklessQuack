using System;
using System.Collections.Generic;

namespace QuarklessContexts.Models.APILogger
{
	[Serializable]
	public class MaxConcurrentRequests
	{
		public bool Enabled { get; set; }
		public int Limit { get; set; }
		public int MaxQueueLength { get; set; }
		public int MaxTimeInQueue { get; set; }
		public string LimitExceededPolicy { get; set; }
		public string[] ExcludePaths { get; set; }
	}
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
	[Serializable]
	public class GeneralRule
	{
		public string Endpoint { get; set; }
		public string Period { get; set; }
		public int Limit { get; set; }
	}
	[Serializable]
	public class IpRateLimitPolicies
	{
		public List<IpRules> IpRules { get; set; }
	}
	[Serializable]
	public class IpRules
	{
		public List<GeneralRule> GeneralRules { get; set; }
		public string Ip { get; set; }
	}

}

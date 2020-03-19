using Quarkless.Base.ApiLogger.Models;
using Quarkless.Base.Auth.Models.Aws;

namespace Quarkless.Models.Shared.Api
{
	public class ApiEnvironmentAccess
	{
		public AWSAccess AwsAccess { get; set; }
		public AWSPool AwsPool { get; set; }
		public XAWSOptions AwsOptions { get; set; }
		public MaxConcurrentRequests MaxConcurrentRequests { get; set; }
		public IpRateLimiting IpRateLimiting { get; set; }
		public IpRateLimitPolicies IpRateLimitPolicies { get; set; }
	}
}

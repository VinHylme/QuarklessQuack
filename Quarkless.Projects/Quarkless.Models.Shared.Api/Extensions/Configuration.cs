using Quarkless.Models.Shared.Extensions;

namespace Quarkless.Models.Shared.Api.Extensions
{
	public class Configuration
	{
		private ApiEnvironmentAccess GetApiEnvironmentAccess()
		{
			var configuration = new Config().Configuration;
			var access = new ConfigurationAccess(configuration);
			return new ApiEnvironmentAccess
			{
				AwsOptions = access.AwsOptions(),
				AwsAccess = access.AwsAccess,
				AwsPool = access.AwsPool,
				IpRateLimiting = access.IpRateLimiting,
				IpRateLimitPolicies = access.IpRateLimitPolicies,
				MaxConcurrentRequests = access.MaxConcurrentRequests
			};
		}
		public ApiEnvironmentAccess Environments => GetApiEnvironmentAccess();
	}
}

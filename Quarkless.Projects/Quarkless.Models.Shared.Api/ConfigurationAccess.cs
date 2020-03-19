using Microsoft.Extensions.Configuration;
using Quarkless.Base.ApiLogger.Models;
using Quarkless.Base.Auth.Models.Aws;

namespace Quarkless.Models.Shared.Api
{
	public class ConfigurationAccess
	{
		private readonly IConfiguration _configuration;
		public ConfigurationAccess(IConfiguration configuration) => _configuration = configuration;

		public AWSAccess AwsAccess => new AWSAccess
		{
			AccessKey = _configuration["AWS:AccessKey"],
			Region = _configuration["AWS:Region"],
			SecretKey = _configuration["AWS:SecretKey"]
		};
		public AWSPool AwsPool => new AWSPool
		{
			AppClientID = _configuration["AWSCredential:AppClientID"],
			AppClientSecret = _configuration["AWSCredential:AppSecretKey"],
			AuthUrl = _configuration["AWSCredential:AuthUrl"] + _configuration["AWSCredential:PoolID"],
			PoolID = _configuration["AWSCredential:PoolID"],
			Region = _configuration["AWS:Region"]
		};
		public XAWSOptions AwsOptions()
		{
			var options = _configuration.GetAWSOptions();
			return new XAWSOptions
			{
				Profile = options.Profile,
				ProfileLocation = options.ProfilesLocation
			};
		}
		public MaxConcurrentRequests MaxConcurrentRequests =>
			_configuration.GetSection("MaxConcurrentRequests").Get<MaxConcurrentRequests>();
		public IpRateLimiting IpRateLimiting => _configuration.GetSection("IpRateLimiting").Get<IpRateLimiting>();
		public IpRateLimitPolicies IpRateLimitPolicies =>
			_configuration.GetSection("IpRateLimitPolicies").Get<IpRateLimitPolicies>();
	}
}
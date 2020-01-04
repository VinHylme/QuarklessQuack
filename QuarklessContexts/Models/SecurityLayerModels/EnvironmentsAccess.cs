using System;
using QuarklessContexts.Models.APILogger;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class EnvironmentsAccess
	{
		public EnvironmentsAccess() { }
		public string BasePath { get; set; }
		public AWSAccess AwsAccess { get; set; }
		public AWSPool AwsPool { get; set; }
		public XAWSOptions AwsOptions { get; set; }
		public MaxConcurrentRequests MaxConcurrentRequests { get; set; }
		public IpRateLimiting IpRateLimiting { get; set; }
		public IpRateLimitPolicies IpRateLimitPolicies { get; set; }
		public string S3BucketName { get; set; }
		public string FrontEnd { get; set; }
		public string SeleniumChromeAddress { get; set; }
		public string NaturalLanguageApiPath { get; set; }
		public string YandexApiKey { get; set; }
		public string DetectApi { get; set; }
		public string ImageSearchEndpoint { get; set; }
		public string RedisConnectionString { get; set; }
		public string ConnectionString { get; set; }
		public string MainDatabase { get; set; }
		public string SchedulerDatabase { get; set; }
		public string ControlDatabase { get; set; }
		public string ContentDatabase { get; set; }
		public string AccountCreationDatabase { get; set; }
		public string TempVideoPath { get; set; }
		public string TempImagePath { get; set; }
		public string FfmpegPath { get; set; }
		public string ApiBasePath { get; set; }
		public string VisionCredentials { get; set; }
	}
}

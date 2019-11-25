﻿using QuarklessContexts;
using QuarklessContexts.Models.APILogger;

namespace Quarkless.Security.AccessorSupport
{
	internal interface IAccessor
	{
		MaxConcurrentRequests MaxConcurrentRequests { get; }
		IpRateLimiting IpRateLimiting { get; }
		IpRateLimitPolicies IpRateLimitPolicies { get; }
		AWSAccess AwsAccess { get; }
		AWSPool AwsPool { get; }
		XAWSOptions AwsOptions();
		string S3BucketName { get; }
		string FrontEnd { get; }
		string SeleniumChromeAddress { get; }
		string NaturalLanguageApiPath { get; }
		string YandexApiKey { get; }
		string DetectApi { get; }
		string ImageSearchEndpoint { get; }
		string RedisConnectionString { get; }
		string ConnectionString { get; }
		string MainDatabase { get; }
		string SchedulerDatabase { get; }
		string ControlDatabase { get; }
		string ContentDatabase { get; }
		string BasePath { get; }
		string TempVideoPath { get;  }
		string TempImagePath { get;  }
		string FfmpegPath { get; }
	}
}
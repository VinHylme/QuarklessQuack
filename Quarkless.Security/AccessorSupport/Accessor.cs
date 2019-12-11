using System;
using Microsoft.Extensions.Configuration;
using QuarklessContexts;
using QuarklessContexts.Models.APILogger;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Security.AccessorSupport
{
	internal interface IInitAccess{
		AvailableClient[] AvailableClients { get; }
	}
	internal class Accessor: IAccessor, IInitAccess
	{
		private readonly IConfiguration _configuration;
		public Accessor(IConfiguration configuration) => _configuration = configuration;

		#region Public Getters
		public string BasePath => Environment.CurrentDirectory.Split("QuarklessQuack")[0] + "QuarklessQuack";
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
		public AvailableClient[] AvailableClients => _configuration.GetSection("AvailableClients").Get<AvailableClient[]>();
		public string S3BucketName => _configuration["AppS3Bucket"];
		public string FrontEnd => _configuration["Endpoints:FrontEnd"];
		public string SeleniumChromeAddress => _configuration["Endpoints:SeleniumChrome"];
		public string NaturalLanguageApiPath => _configuration["APIServices:NaturalLanguageProcessing"];
		public string YandexApiKey => _configuration["APIServices:Yandex"];
		public string DetectApi => _configuration["APIServices:DetectLanguageAPI"];
		public string ImageSearchEndpoint => _configuration["Endpoints:ImageSearchEndpointGoogle"];
		public string RedisConnectionString => _configuration["ConnectionStrings:Redis"];
		public string ConnectionString => _configuration["ConnectionStrings:MongoClientStrings"];
		public string MainDatabase => _configuration["ConnectionStrings:DatabaseNames:Accounts"];
		public string SchedulerDatabase => _configuration["ConnectionStrings:DatabaseNames:Scheduler"];
		public string ControlDatabase => _configuration["ConnectionStrings:DatabaseNames:Control"];
		public string ContentDatabase => _configuration["ConnectionStrings:DatabaseNames:Content"];
		public string TempVideoPath => _configuration["MediaPath:videosTempPath"];
		public string TempImagePath => _configuration["MediaPath:imagesTempPath"];
		public string FfmpegPath => _configuration["Ffmpeg"];
		public string ApiBasePath => _configuration["Endpoints:ApiBasePath"];
		public string AutomatorEndPoint => _configuration["Endpoints:AutomatorPath"];

		public MaxConcurrentRequests MaxConcurrentRequests =>
			_configuration.GetSection("MaxConcurrentRequests").Get<MaxConcurrentRequests>();
		public IpRateLimiting IpRateLimiting => _configuration.GetSection("IpRateLimiting").Get<IpRateLimiting>();
		public IpRateLimitPolicies IpRateLimitPolicies =>
			_configuration.GetSection("IpRateLimitPolicies").Get<IpRateLimitPolicies>();
		#endregion
	}
}

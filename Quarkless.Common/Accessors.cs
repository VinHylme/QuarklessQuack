using System;
using Microsoft.Extensions.Configuration;
using QuarklessContexts;
using System.IO;

namespace Quarkless.Common
{
	public class Accessors
	{
		public AWSAccess AWSAccess { get; set; }
		public AWSPool AWSPool { get; set; }
		private readonly IConfiguration _configuration;
		public Accessors(IConfiguration configuration)
		{
			_configuration = configuration;
			AWSPool = new AWSPool()
			{
				AppClientID = configuration["AWSCredential:AppClientID"],
				AppClientSecret = configuration["AWSCredential:AppSecretKey"],
				AuthUrl = configuration["AWSCredential:AuthUrl"] + configuration["AWSCredential:PoolID"],
				PoolID = configuration["AWSCredential:PoolID"],
				Region = configuration["AWS:Region"]
			};
			AWSAccess = new AWSAccess
			{
				AccessKey = configuration["AWS:AccessKey"],
				Region = configuration["AWS:Region"],
				SecretKey = configuration["AWS:SecretKey"]
			};
		}

		public string S3BucketName => _configuration["AppS3Bucket"];
		public static string BasePath => Environment.CurrentDirectory.Split("QuarklessQuack")[0] + "QuarklessQuack";
		public string GoogleCredentials(string path)
		{
			return File.ReadAllText(path);
		}
		public string FrontEnd => _configuration["Endpoints:FrontEnd"];
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
	}
}

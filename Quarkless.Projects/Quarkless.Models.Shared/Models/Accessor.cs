using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Quarkless.Models.Shared.Models
{
	public class Accessor
	{
		private readonly IConfiguration _configuration;
		private const string VISION_CREDENTIALS = "quitic.visionai.json";
		public Accessor(IConfiguration configuration) => _configuration = configuration;

		#region Public Getters
		public string BasePath => Environment.CurrentDirectory.Split("QuarklessQuack")[0] + "QuarklessQuack";
		public string S3BucketName => _configuration["AppS3Bucket"];
		public string FrontEnd => _configuration["Endpoints:FrontEnd"];
		public string SeleniumChromeAddress => _configuration["Endpoints:SeleniumChrome"];
		public string NaturalLanguageApiPath => _configuration["APIServices:NaturalLanguageProcessing"];
		public string YandexApiKey => _configuration["APIServices:Yandex"];
		public string DetectApi => _configuration["APIServices:DetectLanguageAPI"];
		public string ImageSearchEndpoint => _configuration["Endpoints:ImageSearchEndpointGoogle"];
		public string GoogleGeocodeApiKey => _configuration["APIServices:GoogleGeocodeApiKey"];
		public string GeonamesApiKey => _configuration["APIServices:GeonamesApiKey"];
		public string IpGeoLocationApiKey => _configuration["APIServices:IpGeoLocationApiKey"];
		public string RedisConnectionString => _configuration["ConnectionStrings:Redis"];
		public string ConnectionString => _configuration["ConnectionStrings:MongoClientStrings"];
		public string MainDatabase => _configuration["ConnectionStrings:DatabaseNames:Accounts"];
		public string SchedulerDatabase => _configuration["ConnectionStrings:DatabaseNames:Scheduler"];
		public string ControlDatabase => _configuration["ConnectionStrings:DatabaseNames:Control"];
		public string ContentDatabase => _configuration["ConnectionStrings:DatabaseNames:Content"];
		public string AccountCreationDatabase => _configuration["ConnectionStrings:DatabaseNames:AccountCreator"];
		public string TempVideoPath => _configuration["MediaPath:videosTempPath"];
		public string TempImagePath => _configuration["MediaPath:imagesTempPath"];
		public string TempAudioPath => _configuration["MediaPath:audioTempPath"];
		public string FfmpegPath => _configuration["Ffmpeg"];
		public string ApiBasePath => _configuration["Endpoints:ApiBasePath"];
		public string AutomatorEndPoint => _configuration["Endpoints:AutomatorPath"];
		public string ProxyHandlerApiEndPoint => _configuration["Endpoints:ProxyHandlerApi"];
		public string JsonStripeCredentials =>"{" + string.Join(", ", _configuration.GetSection("Stripe")
			.GetChildren()
			.AsEnumerable()
			.Select(_=> $"\"{_.Key}\": \"{_.Value ?? ""}\"")) +"}";
		public string VisionCredentials(string path) => File.ReadAllText(Path.Combine(path,VISION_CREDENTIALS));
		#endregion
	}
}

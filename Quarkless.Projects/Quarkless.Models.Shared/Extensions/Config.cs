using System;
using System.Dynamic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Quarkless.Models.Shared.Enums;
using Quarkless.Models.Shared.Models;

namespace Quarkless.Models.Shared.Extensions
{
	public class Config
	{
		#region Private Constants
		private const string ENV_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
		private const string ENV_TYPE = "DOTNET_ENV_RELEASE";
		#endregion

		#region Internal Functions
		internal string ReferencePath()
		{
			// const string localHostRefPath = @"References\AppSettings";
			// const string dockerRefPath = @"../src/References/AppSettings";
			//
			// var referencesFilePath = !InDockerContainer 
			// 	? Path.Combine(SolutionPath, localHostRefPath) 
			// 	: dockerRefPath;
			return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFiles"));
		}
		internal IConfiguration GetConfiguration()
		{
			const string fileName = "appsettings{0}.json";
			var configurationBuilder = new ConfigurationBuilder();
			var path = ReferencePath();
			configurationBuilder.SetBasePath(path);

			switch (EnvironmentType)
			{
				case EnvironmentType.Development:
					configurationBuilder.AddJsonFile(string.Format(fileName, ""));
					break;
				case EnvironmentType.Production:
					configurationBuilder.AddJsonFile(string.Format(fileName, ".prod"));
					break;
				case EnvironmentType.Local when !InDockerContainer:
					configurationBuilder.AddJsonFile(string.Format(fileName, ".local"));
					break;
				case EnvironmentType.Local when InDockerContainer:
					configurationBuilder.AddJsonFile(string.Format(fileName, ".docker.local"));
					break;
				case EnvironmentType.None:
					throw new Exception("Please enter the development type");
			}

			return configurationBuilder.Build();
		}
		internal EnvironmentsAccess GetEnvironmentsDetails()
		{
			var access = new Accessor(GetConfiguration());
			return new EnvironmentsAccess
			{
				ApiBasePath = access.ApiBasePath,
				BasePath = access.BasePath,
				ConnectionString = access.ConnectionString,
				ContentDatabase = access.ContentDatabase,
				DetectApi = access.DetectApi,
				S3BucketName = access.S3BucketName,
				StatisticsDatabase = access.StatisticsDatabase,
				NaturalLanguageApiPath = access.NaturalLanguageApiPath,
				GeonamesApiKey = access.GeonamesApiKey,
				GoogleGeocodeApiKey = access.GoogleGeocodeApiKey,
				IpGeoLocationApiKey = access.IpGeoLocationApiKey,
				YandexApiKey = access.YandexApiKey,
				RedisConnectionString = access.RedisConnectionString,
				MainDatabase = access.MainDatabase,
				ControlDatabase = access.ControlDatabase,
				AccountCreationDatabase = access.AccountCreationDatabase,
				FrontEnd = access.FrontEnd,
				ProxyHandlerApiEndPoint = access.ProxyHandlerApiEndPoint,
				SeleniumChromeAddress = access.SeleniumChromeAddress,
				TempImagePath = access.TempImagePath,
				TempVideoPath = access.TempVideoPath,
				TempAudioPath = access.TempAudioPath,
				FfmpegPath = access.FfmpegPath,
				VisionCredentials = access.VisionCredentials(ReferencePath()),
				ImageSearchEndpoint = access.ImageSearchEndpoint,
				JsonStripeCredentials = access.JsonStripeCredentials
			};
		}
		#endregion

		public string SolutionPath => @$"{Directory.GetCurrentDirectory().Split("QuarklessQuack")[0]}QuarklessQuack\";

		public bool InDockerContainer
		{
			get
			{
				var parseResults = bool.TryParse(Environment.GetEnvironmentVariable(ENV_RUNNING_IN_CONTAINER), out var inDocker);
				return parseResults && inDocker;
			}
		}
		public EnvironmentType EnvironmentType
		{
			get
			{
				var environmentType = Environment.GetEnvironmentVariable(ENV_TYPE);
				if (string.IsNullOrEmpty(environmentType))
					return EnvironmentType.None;

				return environmentType switch
				{
					"dev" => EnvironmentType.Development,
					"prod" => EnvironmentType.Production,
					"local" => EnvironmentType.Local,
					_ => EnvironmentType.None
				};
			}
		}
		public EnvironmentsAccess Environments => GetEnvironmentsDetails();
		public IConfiguration Configuration => GetConfiguration();
	}
}

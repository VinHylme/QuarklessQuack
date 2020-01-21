using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Quarkless.Models.Shared.Models;

namespace Quarkless.Models.Shared.Extensions
{
	internal class ConfigInit
	{
		internal string ReferencePath()
		{
			var parseRes = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker);

			string referencesFilePath;
			if (!parseRes || !inDocker)
			{
				var currentDirectory = Directory.GetCurrentDirectory().Split("bin")[0];
				referencesFilePath = Path.Combine(currentDirectory, "../../../References/AppSettings");
			}
			else
			{
				referencesFilePath = "../src/References/AppSettings";
			}

			return Path.GetFullPath(referencesFilePath);
		}
		internal IConfiguration GetConfiguration()
		{
			var parseEnvRes = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_ENV_RELEASE"), out var inRelease);
			const string fileName = "appsettings{0}.json";
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(ReferencePath());

			if (!parseEnvRes || !inRelease)
			{
				configurationBuilder.AddJsonFile(string.Format(fileName, ""));
			}
			else
			{
				configurationBuilder.AddJsonFile(string.Format(fileName, "prod"));
			}

			return configurationBuilder.Build();
		}
		internal EnvironmentsAccess GetEnvironmentsDetails()
		{
			var access = new Accessor(new ConfigInit().GetConfiguration());
			return new EnvironmentsAccess
			{
				ApiBasePath = access.ApiBasePath,
				BasePath = access.BasePath,
				ConnectionString = access.ConnectionString,
				ContentDatabase = access.ContentDatabase,
				DetectApi = access.DetectApi,
				S3BucketName = access.S3BucketName,
				SchedulerDatabase = access.SchedulerDatabase,
				NaturalLanguageApiPath = access.NaturalLanguageApiPath,
				YandexApiKey = access.YandexApiKey,
				RedisConnectionString = access.RedisConnectionString,
				MainDatabase = access.MainDatabase,
				ControlDatabase = access.ControlDatabase,
				AccountCreationDatabase = access.AccountCreationDatabase,
				FrontEnd = access.FrontEnd,
				SeleniumChromeAddress = access.SeleniumChromeAddress,
				TempImagePath = access.TempImagePath,
				TempVideoPath = access.TempVideoPath,
				TempAudioPath = access.TempAudioPath,
				FfmpegPath = access.FfmpegPath,
				VisionCredentials = access.VisionCredentials(ReferencePath()),
				ImageSearchEndpoint = access.ImageSearchEndpoint
			};
		}
	}
}
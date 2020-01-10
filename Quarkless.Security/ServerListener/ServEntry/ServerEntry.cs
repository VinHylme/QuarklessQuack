using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Quarkless.Security.AccessorSupport;
using Quarkless.Security.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Security.ServerListener.ServEntry
{
	internal sealed class ServerEntry
	{
		private string _fileName = "appsettings{0}.json";
		private bool _isValid = false;
		internal bool IsValidClient(AvailableClient clientFrom)
		{
			var clients = Initialise(clientFrom);
			foreach (var client in clients)
			{
				if (client.GetHashCode() != clientFrom.GetHashCode())
					continue;
				_isValid = true;
				break;
			}
			return _isValid;
		}

		public bool IsValidated => _isValid;
		private AvailableClient[] Initialise(AvailableClient client)
		{
			return new Accessor(MakeConfigurationBuilder(".init")
				.Unlock(client.GetHashCode())
				.Build()).AvailableClients;
		}
		public EndPoints PublicEndpoints(bool useLocal)
		{
			if(!_isValid) throw new Exception("Invalid Client");
			var access = GetEnvData(useLocal);
			return new EndPoints
			{
				FrontEnd = access.FrontEnd,
				RedisCon = access.RedisConnectionString,
				AutomatorService = access.AutomatorEndPoint
			};
		}
		public EnvironmentsAccess RequestEnvData(bool useLocal)
		{
			var a = GetEnvData(useLocal);
			return new EnvironmentsAccess
			{
				AwsOptions = a.AwsOptions(),
				ApiBasePath = a.ApiBasePath,
				AwsAccess = a.AwsAccess,
				AwsPool = a.AwsPool,
				BasePath = a.BasePath,
				ConnectionString = a.ConnectionString,
				ContentDatabase = a.ContentDatabase,
				DetectApi = a.DetectApi,
				S3BucketName = a.S3BucketName,
				SchedulerDatabase = a.SchedulerDatabase,
				NaturalLanguageApiPath = a.NaturalLanguageApiPath,
				YandexApiKey = a.YandexApiKey,
				RedisConnectionString = a.RedisConnectionString,
				MainDatabase = a.MainDatabase,
				ControlDatabase = a.ControlDatabase,
				AccountCreationDatabase = a.AccountCreationDatabase,
				FrontEnd = a.FrontEnd,
				IpRateLimiting = a.IpRateLimiting,
				IpRateLimitPolicies = a.IpRateLimitPolicies,
				ImageSearchEndpoint = a.ImageSearchEndpoint,
				MaxConcurrentRequests = a.MaxConcurrentRequests,
				SeleniumChromeAddress = a.SeleniumChromeAddress,
				TempImagePath = a.TempImagePath,
				TempVideoPath = a.TempVideoPath,
				TempAudioPath = a.TempAudioPath,
				FfmpegPath = a.FfmpegPath,
				VisionCredentials = a.VisionCredentials
			};
		}
		private IAccessor GetEnvData(bool useLocal)
		{
			if(_isValid)
				return new Accessor(MakeConfigurationBuilder(useLocal ? ".local" : "").Unlock(0).Build());
			throw new Exception("Invalid Client");
		}
		private IConfigurationBuilder MakeConfigurationBuilder(string envType = "prod")
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile(string.Format(_fileName, envType));
		}
	}
}

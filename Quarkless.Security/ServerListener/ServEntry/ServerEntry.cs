using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Quarkless.Security.AccessorSupport;
using Quarkless.Security.Extensions;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Security.ServerListener.ServEntry
{
	internal sealed class ClientSocket
	{
		private readonly ServerEntry _serverEntry;
		private readonly Socket _client;
		public ClientSocket(Socket client)
		{
			_client = client;
			_serverEntry = new ServerEntry();
		}

		public Socket GetClientSocket => _client;
		public bool ValidateClient(AvailableClient availableClient) => _serverEntry.IsValidClient(availableClient);
		public bool IsValidated => _serverEntry.IsValidated;
		public string GetEnvData() => _serverEntry.RequestEnvData().Serialize();
		public string GetPublicEndpoints() => _serverEntry.PublicEndpoints().Serialize();
	}
	internal sealed class ClientSockets : IEnumerable<ClientSocket>
	{
		private readonly List<ClientSocket> _clients;
		public ClientSockets()
		{
			_clients = new List<ClientSocket>();
		}

		public void Clean() => _clients.Clear();
		public void AddClient(ClientSocket client)
		{
			if(!_clients.Contains(client))
				_clients.Add(client);
		}
		public object this[int index] => _clients.ElementAtOrDefault(index);
		public ClientSocket GetClient(Socket client) => _clients.Find(c => c.GetClientSocket == client);
		public IEnumerator<ClientSocket> GetEnumerator()
		{
			return _clients.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	internal sealed class ServerEntry
	{
		private const string _fileName = "appsettings{0}.json";
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
		
		public EndPoints PublicEndpoints()
		{
			if(!_isValid) throw new Exception("Invalid Client");
			var access = GetEnvData();
			return new EndPoints
			{
				FrontEnd = access.FrontEnd,
				RedisCon = access.RedisConnectionString
			};
		}

		public EnvironmentsAccess RequestEnvData()
		{
			var a = GetEnvData();
			return new EnvironmentsAccess
			{
				AwsOptions = a.AwsOptions(),
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
				FrontEnd = a.FrontEnd,
				IpRateLimiting = a.IpRateLimiting,
				IpRateLimitPolicies = a.IpRateLimitPolicies,
				ImageSearchEndpoint = a.ImageSearchEndpoint,
				MaxConcurrentRequests = a.MaxConcurrentRequests,
				SeleniumChromeAddress = a.SeleniumChromeAddress,
				TempImagePath = a.TempImagePath,
				TempVideoPath = a.TempVideoPath,
				FfmpegPath = a.FfmpegPath
			};
		}
		private IAccessor GetEnvData()
		{
			if(_isValid)
				return new Accessor(MakeConfigurationBuilder().Unlock(0).Build());
			throw new Exception("Invalid Client");
		}
		private IConfigurationBuilder MakeConfigurationBuilder(string name = "")
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile(string.Format(_fileName, name));
		}
	}
}

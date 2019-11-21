using System;
using System.IO;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using QuarklessContexts.Models.SecurityLayerModels;
using QuarklessLogic.QueueLogic.Jobs.Filters;
using StackExchange.Redis;

namespace Quarkless.Tasks
{
	class Program
	{
		private const string CLIENT_SECTION = "Client";
		private static IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile("appsettings.json").Build();
		}
		private static void InitialiseClientServices()
		{
			var cIn = new ClientRequester();
			if (!cIn.TryConnect().GetAwaiter().GetResult())
				throw new Exception("Invalid Client");

			var caller = MakeConfigurationBuilder().GetSection(CLIENT_SECTION).Get<AvailableClient>();

			var validate = cIn.Send(new InitCommandArgs
			{
				Client = caller,
				CommandName = "Validate Client"
			});
			if (!(bool)validate)
				throw new Exception("Could not validated");

			var services = (IServiceCollection) cIn.Send(new BuildCommandArgs()
			{
				CommandName = "Build Services",
				ServiceTypes = new[]
				{
					ServiceTypes.AddHangFrameworkServices
				}
			});
			var endPoints = (EndPoints) cIn.Send(new GetPublicEndpointCommandArgs
			{
				CommandName = "Get Public Endpoints",
			});
			var connectionMultiplexer = ConnectionMultiplexer.Connect(endPoints.RedisCon);

			GlobalConfiguration.Configuration.UseRedisStorage(connectionMultiplexer, new Hangfire.Redis.RedisStorageOptions
			{
				Prefix = "Timeline",
				SucceededListSize = 5000,
				DeletedListSize = 1000,
				ExpiryCheckInterval = TimeSpan.FromHours(1),
				InvisibilityTimeout = TimeSpan.FromMinutes(30),
				UseTransactions = true
			}).WithJobExpirationTimeout(TimeSpan.FromDays(1));
			GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(services.BuildServiceProvider(false)));
			GlobalConfiguration.Configuration.UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings()
			{
				ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			});
			GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());
		}

		static void Main(string[] args)
		{
			#region Mongo Storage
			//GlobalConfiguration.Configuration.UseMongoStorage(_accessors.ConnectionString, _accessors.SchedulerDatabase, new MongoStorageOptions()
			//{
			//	Prefix = "Timeline",
			//	CheckConnection = false,
			//	MigrationOptions = new MongoMigrationOptions(MongoMigrationStrategy.Migrate)
			//	{
			//		Strategy = MongoMigrationStrategy.Migrate,
			//		BackupStrategy = MongoBackupStrategy.Collections
			//	},
			//	QueuePollInterval = TimeSpan.Zero,
			//});
			#endregion

			InitialiseClientServices();
			var jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = string.Format("TASK_{0}.{1}",Environment.MachineName,Guid.NewGuid().ToString()),
				//Activator = new WorkerActivator(services.BuildServiceProvider(false)),
			};
			using (var server = new BackgroundJobServer(jobServerOptions))
			{
				Console.WriteLine("hangfire server started");
				Console.ReadKey();
			}
		}
	}
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;
		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
}

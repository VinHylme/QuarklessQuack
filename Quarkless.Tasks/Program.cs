using System;
using System.IO;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Queue.Jobs.Filters;
using StackExchange.Redis;

namespace Quarkless.Tasks
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			var settingPath = Path.GetFullPath(Path.Combine(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			Accessors accessors = new Accessors(configuration);
			var Redis = ConnectionMultiplexer.Connect(accessors.RedisConnectionString);

			services.AddHangFrameworkServices(accessors);

			GlobalConfiguration.Configuration.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions{
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


			BackgroundJobServerOptions jobServerOptions = new BackgroundJobServerOptions
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

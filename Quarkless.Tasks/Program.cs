using System;
using System.IO;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Queue;
using Quarkless.Queue.Jobs.Filters;

namespace Quarkless.Tasks
{
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;
		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
	class Program
	{
		static void Main(string[] args)
		{
			/*
			var settingPath = Path.GetFullPath(Path.Combine(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			Accessors accessors = new Accessors(configuration);
			var services = new ServiceCollection();

			services.AddHangFrameworkServices(accessors);
			GlobalConfiguration.Configuration.UseMongoStorage(accessors.ConnectionString, accessors.SchedulerDatabase,
				new MongoStorageOptions{
					Prefix = "Timeline",
					CheckConnection = false,
					MigrationOptions = new MongoMigrationOptions
					{
						Strategy = MongoMigrationStrategy.Migrate,
						BackupStrategy = MongoBackupStrategy.Collections
					}
				});

			GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(services.BuildServiceProvider(false)));
			GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());

			BackgroundJobServerOptions jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount,
				ServerName = string.Format("{0}.{1}",Environment.MachineName,Guid.NewGuid().ToString()),
				Activator = new WorkerActivator(services.BuildServiceProvider(false))
			};
			using (var server = new BackgroundJobServer(jobServerOptions))
			{
				Console.WriteLine("hangfire server started");
				Console.ReadKey();
			}*/
		}
	}
}

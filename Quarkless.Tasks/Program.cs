using System;
using System.IO;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Queue;
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
			var settingPath = Path.GetFullPath(Path.Combine(@"..\..\..\..\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			Accessors accessors = new Accessors(configuration);
			var services = new ServiceCollection();

			services.AddHangFrameworkServices(accessors);

			GlobalConfiguration.Configuration.UseMongoStorage(accessors.ConnectionString, accessors.ControlDatabase,new MongoStorageOptions{ 
				CheckConnection = false
			});
			GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(services.BuildServiceProvider(false)));

			using (var server = new BackgroundJobServer())
			{
				Console.WriteLine("hangfire server started");
				Console.ReadKey();
			}
		}
	}
}

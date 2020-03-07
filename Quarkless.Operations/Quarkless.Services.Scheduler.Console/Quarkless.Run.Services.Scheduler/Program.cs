using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Run.Services.Scheduler.Extensions;
using StackExchange.Redis;
using Environment = System.Environment;

namespace Quarkless.Run.Services.Scheduler
{
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _container;
		public WorkerActivator(IServiceProvider container)
		{
			_container = container ?? throw new ArgumentNullException(nameof(container));
		}

		public override object ActivateJob(Type jobType) => _container.GetService(jobType);
	}

	class Program
	{
		private const string REDIS_DB_NAME = "Timeline";

		static async Task Main(string[] args)
		{
			if (args.Length > 0)
			{
				Environment.SetEnvironmentVariable("DOTNET_ENV_RELEASE", args[0]);
				Environment.SetEnvironmentVariable("USER_ID", args[1]);
			}

			if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENV_RELEASE")))
			{
				Console.WriteLine("Incorrect Release type");
				return;
			}

			if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USER_ID")))
			{
				Console.WriteLine("Invalid User");
				return;
			}

			var accountId = Environment.GetEnvironmentVariable("USER_ID");

			if (string.IsNullOrEmpty(accountId)) return;

			var services = new ServiceCollection();
			services.IncludeHangFrameworkServices();
			services.IncludeRepositoryAndConfigs();
			services.IncludeLogic();
			
			var serializerSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			};

			var redis = ConnectionMultiplexer.Connect(new Config().Environments.RedisConnectionString);

			var serviceProvider = services.BuildServiceProvider();
			using var scope = serviceProvider.CreateScope();
			var accountLogic = scope.ServiceProvider.GetService<IInstagramAccountLogic>();
			var instagramAccounts = await accountLogic.GetInstagramAccountsOfUser(accountId);

			if (instagramAccounts == null || !instagramAccounts.Any())
			{
				Console.WriteLine("No accounts found for this user, exiting...");
				return;
			}

			GlobalConfiguration.Configuration.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
			{
				Db = 1,
				Prefix = REDIS_DB_NAME, //$"{REDIS_DB_NAME}_{accountId}_",
				SucceededListSize = 10000,
				DeletedListSize = 10000,
				InvisibilityTimeout = TimeSpan.FromMinutes(30),
				FetchTimeout = TimeSpan.FromMinutes(30),
				UseTransactions = true
			});

			var activator = new WorkerActivator(serviceProvider);
			GlobalConfiguration.Configuration.UseActivator(activator);
			JobActivator.Current = activator;

			GlobalConfiguration.Configuration.UseSerializerSettings(serializerSettings);
			
			var jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = $"ISE_{Environment.MachineName}.{Guid.NewGuid().ToString()}",
			};

			serviceProvider.ResetHangfireJobs();
			using var server = new BackgroundJobServer(jobServerOptions);
			Console.WriteLine($"Server {jobServerOptions.ServerName} started...");
			Console.Read();
		}
	}
}

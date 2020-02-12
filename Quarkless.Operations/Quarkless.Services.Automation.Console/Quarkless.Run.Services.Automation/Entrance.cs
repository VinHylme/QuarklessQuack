using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Logic.Services.Automation;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Run.Services.Automation.Extensions;
using StackExchange.Redis;

namespace Quarkless.Run.Services.Automation
{
	public class Entrance
	{
		static void Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			var userId = environmentVariables["USER_ID"].ToString();
			var instagramId = environmentVariables["USER_INSTAGRAM_ACCOUNT"].ToString();
			var workerType = int.Parse(environmentVariables["USER_WORKER_ACCOUNT_TYPE"].ToString());

			var services = new ServiceCollection();
			services.IncludeHangFrameworkServices();
			services.IncludeLogicServices();
			services.IncludeConfigurators();
			services.IncludeRepositories();
			services.IncludeHandlers();
			services.IncludeContexts();
			services.IncludeEventServices();
			services.AddSingleton<IAgentTests, AgentTests>();
			services.AddSingleton<IWorkerManager, WorkerManager>
			(s => new WorkerManager(s.GetService<IApiClientContext>(),
				s.GetService<IInstagramAccountLogic>(),
				s.GetService<IResponseResolver>(), 1, workerType));

			var redis = ConnectionMultiplexer.Connect(new Config().Environments.RedisConnectionString);
			GlobalConfiguration.Configuration.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
			{
				Db = 1,
				Prefix = "Timeline",
				SucceededListSize = 5000,
				DeletedListSize = 1000,
				ExpiryCheckInterval = TimeSpan.FromHours(1),
				InvisibilityTimeout = TimeSpan.FromMinutes(30),
				FetchTimeout = TimeSpan.FromMinutes(30),
				UseTransactions = true
			})
				.WithJobExpirationTimeout(TimeSpan.FromHours(12));

			System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx =>
			{
				ShutDownInstance.ShutDown();
			};

			var results = WithExceptionLogAsync(async () =>
			{
				using var scope = services.BuildServiceProvider().CreateScope();
				await scope.ServiceProvider.GetService<IAgentManager>().Start(userId, instagramId);
				//await scope.ServiceProvider.GetService<IAgentTests>().StartTests();
			});

			Task.WaitAll(results);
		}
		
		private static Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
	}
}

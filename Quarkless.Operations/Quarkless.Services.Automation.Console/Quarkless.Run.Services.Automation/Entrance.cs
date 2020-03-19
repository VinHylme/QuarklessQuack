using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Base.Actions.Logic.Factory.ActionExecute.Manager;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Run.Services.Automation.Models.Extensions;
using StackExchange.Redis;
using AgentTests = Quarkless.Run.Services.Automation.Logic.AgentTests;
using IAgentManager = Quarkless.Run.Services.Automation.Models.Interfaces.IAgentManager;
using IAgentTests = Quarkless.Run.Services.Automation.Models.Interfaces.IAgentTests;

namespace Quarkless.Run.Services.Automation
{
	public class Entrance
	{
		private const string REDIS_DB_NAME = "Timeline";
		static void Main(string[] args)
		{
			var shouldTest = false;

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

			services.AddSingleton<IWorkerManager, WorkerManager>(s => new WorkerManager(s.GetService<IApiClientContext>(),
				s.GetService<IInstagramAccountLogic>(),
				s.GetService<IResponseResolver>(), 1, workerType));

			var redis = ConnectionMultiplexer.Connect(new Config().Environments.RedisConnectionString);
			GlobalConfiguration.Configuration.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
			{
				Db = 1,
				Prefix = REDIS_DB_NAME,
				SucceededListSize = 10000,
				DeletedListSize = 10000,
				InvisibilityTimeout = TimeSpan.FromMinutes(30),
				FetchTimeout = TimeSpan.FromMinutes(30),
				UseTransactions = true
			}).WithJobExpirationTimeout(TimeSpan.FromHours(12));

			System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx =>
			{
				ShutDownInstance.ShutDown();
			};

			if (shouldTest)
			{
				services.AddSingleton<IAgentTests, AgentTests>();
				services.AddTransient<IActionExecuteFactory, ActionExecuteFactoryManager>();
			}

			var results = WithExceptionLogAsync(async () =>
			{
				using var scope = services.BuildServiceProvider().CreateScope();

				if(shouldTest)
					await scope.ServiceProvider.GetService<IAgentTests>().StartTests();
				else
					await scope.ServiceProvider.GetService<IAgentManager>().Start(userId, instagramId);
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

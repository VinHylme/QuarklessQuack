using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Shared.Extensions;
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
			
			var services = new ServiceCollection();

			services.IncludeHangFrameworkServices();
			services.IncludeLogicServices();
			services.IncludeAuthHandlers();
			services.IncludeConfigurators();
			services.IncludeRepositories();
			services.IncludeHandlers();
			services.IncludeContexts();
			services.IncludeEventServices();

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
			}).WithJobExpirationTimeout(TimeSpan.FromHours(12));

			var results = WithExceptionLogAsync(async () =>
			{
				using var scope = services.BuildServiceProvider().CreateScope();
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
